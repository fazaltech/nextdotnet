"""Integration checks against the isolated database selected by verify-service.ps1."""
import concurrent.futures
import json
import os
from pathlib import Path
import subprocess
import time
import urllib.request
import urllib.error
import uuid

root = Path(__file__).resolve().parents[2]
base = 'http://127.0.0.1:5187/api/'
token = ''

def call(path, body=None, method=None, expected=200, auth=None):
    req = urllib.request.Request(base + path, data=None if body is None else json.dumps(body).encode(),
        headers={'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (token if auth is None else auth)},
        method=method or ('GET' if body is None else 'POST'))
    try:
        with urllib.request.urlopen(req, timeout=25) as response:
            status, raw = response.status, response.read()
    except urllib.error.HTTPError as error:
        status, raw = error.code, error.read()
    assert status == expected, f'{path}: expected {expected}, got {status}: {raw[:700]!r}'
    return json.loads(raw) if raw else None

def op(action, expected=200, **values):
    values.setdefault('requestKey', str(uuid.uuid4()))
    result = call('service/' + action, values, expected=expected)
    return result.get('id') if isinstance(result, dict) else None

def rows(section):
    # Dapper dictionary property casing follows database column names.
    return [{k[0].lower()+k[1:]:v for k,v in x.items()} for x in call('service/'+section)]

log = open(root/'backend/tests/verification.log', 'w', encoding='utf-8')
process = subprocess.Popen(['dotnet', str(root/'backend/src/Backend.Api/bin/Debug/net10.0/Backend.Api.dll'), '--urls', 'http://127.0.0.1:5187'],
    cwd=root/'backend/src/Backend.Api', stdout=log, stderr=subprocess.STDOUT)
try:
    for _ in range(90):
        if process.poll() is not None: raise RuntimeError('API startup failed; inspect backend/tests/verification.log')
        try:
            token = call('auth/login', {'userNameOrEmail':'admin', 'password':os.environ['Bootstrap__AdminPassword']})['accessToken']
            break
        except urllib.error.URLError: time.sleep(0.5)
    assert token, 'API did not start'
    category = call('food-categories', {'name':'Verification','description':'Test','isActive':True},expected=201)['id']
    menu = call('menu-items', {'categoryId':category,'name':'Recipe dish','price':20,'isAvailable':True,'isActive':True},expected=201)['id']
    ingredient = call('inventory/items', {'name':'Rice','unit':'kg','quantity':0,'reorderLevel':2,'unitCost':4,'isActive':True},expected=201)['id']
    table = call('tables', {'tableNumber':'TEST1','capacity':4,'isOccupied':False,'isActive':True},expected=201)['id']
    guest = call('customers', {'fullName':'Guest','isActive':True},expected=201)['id']
    guest2 = call('customers', {'fullName':'Guest 2','isActive':True},expected=201)['id']
    op('recipe',menuItemId=menu,inventoryItemId=ingredient,quantity=0.5)
    session=op('open-session',tableId=table)
    op('assign-seat',sessionId=session,tableId=table,customerId=guest,seat=1)
    op('assign-seat',sessionId=session,tableId=table,customerId=guest,seat=2)
    op('assign-seat',sessionId=session,tableId=table,customerId=guest2,seat=1)
    order=op('submit-order',sessionId=session,tableId=table,items=[{'menuItemId':menu,'quantity':2,'customerId':guest,'seat':1},{'menuItemId':menu,'quantity':1,'customerId':guest2,'seat':1}])
    op('approve-order',id=order,expected=409)
    assert rows('stock')[0]['quantity']==0
    op('stock-count',inventoryItemId=ingredient,quantity=10,reason='Opening physical count')
    key=str(uuid.uuid4())
    with concurrent.futures.ThreadPoolExecutor() as pool:
        futures=[pool.submit(op,'approve-order',id=order,requestKey=key) for _ in range(3)]
        for future in futures: future.result()
    assert rows('stock')[0]['quantity']==8.5, 'Approval deducted more than once'
    op('approve-order',id=order)
    assert rows('stock')[0]['quantity']==8.5
    items=[x for x in rows('items') if x['orderId']==order]
    op('cancel-item',id=items[1]['id'],reason='Changed mind before preparation')
    assert rows('stock')[0]['quantity']==8.5+items[1]['quantity']*0.5
    item=items[0]
    for status in ['Preparing','Ready','Served']:op('item-status',id=item['id'],status=status)
    assert rows('tables')[0]['isOccupied'], 'Serving must not release table'
    bill=op('bill',sessionId=session,itemIds=[item['id']])
    op('bill',sessionId=session,itemIds=[item['id']],expected=409)
    total=next(x for x in rows('bills') if x['id']==bill)['totalAmount']
    op('pay',id=bill,amount=total+1,expected=409)
    op('pay',id=bill,amount=total/2)
    assert next(x for x in rows('bills') if x['id']==bill)['status']==2
    key=str(uuid.uuid4())
    op('pay',id=bill,amount=total/2,requestKey=key)
    op('pay',id=bill,amount=total/2,requestKey=key)
    assert next(x for x in rows('bills') if x['id']==bill)['balance']==0
    op('cancel-item',id=item['id'],reason='Cannot edit paid order',expected=409)
    op('close-session',sessionId=session)
    assert not rows('tables')[0]['isOccupied']
    op('refund',id=bill,amount=total,reason='Approved full refund')
    refunded=next(x for x in rows('bills') if x['id']==bill)
    assert refunded['status']==5 and refunded['balance']==0 and refunded['refundAmount']==total
    op('void-bill',id=bill,reason='Refunded bill void')
    op('table-status',id=table,status='Available')
    session2=op('open-session',tableId=table)
    order2=op('submit-order',sessionId=session2,tableId=table,items=[{'menuItemId':menu,'quantity':1},{'menuItemId':menu,'quantity':1}])
    op('approve-order',id=order2)
    lines2=[x for x in rows('items') if x['orderId']==order2]
    op('item-status',id=lines2[0]['id'],status='Preparing')
    stock_before_waste=rows('stock')[0]['quantity']
    op('cancel-item',id=lines2[0]['id'],reason='Burned after preparation started')
    assert rows('stock')[0]['quantity']==stock_before_waste, 'Prepared cancellations must not restore ingredients'
    assert any(x['movementKind']=='Waste' for x in rows('movements'))
    op('close-session',sessionId=session2,expected=409)
    for status in ['Preparing','Ready','Served']:op('item-status',id=lines2[1]['id'],status=status)
    for setting,value in [('TaxPercent','10'),('ServiceChargePercent','5'),('DiscountPercent','10'),('PaymentMethods','1,3')]:
        call('settings',{'settingKey':setting,'settingValue':value,'isActive':True},expected=201)
    bill2=op('bill',sessionId=session2,itemIds=[lines2[1]['id']])
    computed=next(x for x in rows('bills') if x['id']==bill2)
    assert computed['subtotal']==20 and computed['discount']==2 and computed['taxAmount']==1.8 and computed['serviceCharge']==0.9 and computed['totalAmount']==20.7
    op('pay',id=bill2,amount=1,method=2,expected=409)
    op('pay',id=bill2,amount=20.7,method=1)
    op('refund',id=bill2,amount=5,method=1,reason='Partial service credit')
    credited=next(x for x in rows('bills') if x['id']==bill2)
    assert credited['balance']==0 and credited['status']==3
    op('close-session',sessionId=session2)
    # Move an order, combine the parties, and allocate separate bills without duplicates.
    table2=call('tables',{'tableNumber':'TEST2','capacity':4,'isOccupied':False,'isActive':True},expected=201)['id']
    table3=call('tables',{'tableNumber':'TEST3','capacity':4,'isOccupied':False,'isActive':True},expected=201)['id']
    table4=call('tables',{'tableNumber':'TEST4','capacity':4,'isOccupied':False,'isActive':True},expected=201)['id']
    session3=op('open-session',tableId=table2)
    session4=op('open-session',tableId=table3)
    op('assign-seat',sessionId=session3,tableId=table2,customerId=guest,seat=1)
    order3=op('submit-order',sessionId=session3,tableId=table2,items=[{'menuItemId':menu,'quantity':1},{'menuItemId':menu,'quantity':1}])
    op('move-table',sessionId=session3,targetId=table4,reason='Larger dining area')
    op('move-order',id=order3,sessionId=session4,tableId=table3,reason='Guest joins other party')
    op('combine-session',sessionId=session3,targetId=session4,reason='Parties combine')
    assert sum(1 for x in rows('tables') if x['sessionId']==session4)==2
    op('approve-order',id=order3)
    lines3=[x for x in rows('items') if x['orderId']==order3]
    for line in lines3:
        for status in ['Preparing','Ready','Served']:op('item-status',id=line['id'],status=status)
        split_bill=op('bill',sessionId=session4,itemIds=[line['id']])
        op('bill',sessionId=session4,itemIds=[line['id']],expected=409)
        split_total=next(x for x in rows('bills') if x['id']==split_bill)['totalAmount']
        op('pay',id=split_bill,amount=split_total)
    op('close-session',sessionId=session4)
    assert all(not x['isOccupied'] for x in rows('tables'))
    supplier=op('supplier',name='Verification supplier')
    purchase=op('purchase',targetId=supplier,reference='PO-TEST',discount=0,tax=0)
    purchase_line=op('purchase-item',id=purchase,inventoryItemId=ingredient,quantity=8,unitCost=5)
    before=rows('stock')[0]['quantity']
    key=str(uuid.uuid4())
    op('receive',id=purchase_line,quantity=3,requestKey=key)
    op('receive',id=purchase_line,quantity=3,requestKey=key)
    assert rows('stock')[0]['quantity']==before+3
    op('receive',id=purchase_line,quantity=6,expected=409)
    op('receive',id=purchase_line,quantity=5)
    op('purchase-payment',id=purchase,amount=40)
    op('correct-purchase-item',id=purchase_line,quantity=9,unitCost=6,reason='Cannot edit received line',expected=409)
    password='User!'+uuid.uuid4().hex+'Aa1'
    user=call('users',{'userName':'employee','email':'employee@example.test','fullName':'Employee','password':password,'roles':['Employee']},expected=201)['id']
    employee=call('auth/login',{'userNameOrEmail':'employee','password':password})['accessToken']
    call('service/approve-order',{'id':order,'requestKey':str(uuid.uuid4())},auth=employee,expected=403)
    call('payments',{'invoiceId':bill,'amount':1,'method':1},auth=employee,expected=403)
    call('users',auth=employee,expected=403)
    assert call('service/orders',auth=employee)==[], 'Employee must not see others orders'
    new_password='Reset!'+uuid.uuid4().hex+'Aa1'
    call(f'users/{user}/reset-password',{'password':new_password},expected=204)
    call('auth/login',{'userNameOrEmail':'employee','password':new_password})
    # A changed permission is honored immediately even with the existing JWT.
    employee_role=next(x for x in call('roles') if x['name']=='Employee')['id']
    original_grants=call(f'roles/{employee_role}/permissions')
    payment_permission=next(x for x in call('roles/permissions') if x['name']=='payments.manage')['id']
    call(f'roles/{employee_role}/permissions',{'permissionIds':original_grants+[payment_permission]},method='PUT',expected=204)
    call('payments',auth=employee)
    call(f'roles/{employee_role}/permissions',{'permissionIds':original_grants},method='PUT',expected=204)
    call('payments',auth=employee,expected=403)
    for section in ['tables','seats','orders','items','kitchen','recipes','stock','movements','suppliers','purchases','purchase-items','bills','receipt-items','audit','sales','sales-items','payment-report','recipe-cost','profit']:
        rows(section)
    assert any(x['action']=='approve-order' for x in rows('audit'))
    socket_env={**os.environ,'RMS_TEST_TOKEN':token}
    socket=subprocess.Popen(['node',str(root/'backend/tests/verify_signalr.cjs')],env=socket_env,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True)
    assert socket.stdout.readline().strip()=='CONNECTED', 'SignalR connection failed'
    op('stock-count',inventoryItemId=ingredient,quantity=rows('stock')[0]['quantity'],reason='SignalR notification verification')
    out,err=socket.communicate(timeout=15)
    assert socket.returncode==0 and 'CHANGED' in out, f'SignalR notification failed: {err}'
    print('PASS: recipe-based tax/service/discount calculations, configured payment methods, prepared cancellation waste, partial refund credits, immediate role permission revocation, SignalR notifications.')
    print('PASS: service lifecycle, shared/multiple seats, insufficient stock rollback, concurrent retry safety, cancellation reversal, partial/duplicate payments, settlement locking, refunds/voids, partial receipt retry safety, permissions, password reset, all service reports.')
finally:
    process.terminate()
    try: process.wait(timeout=10)
    except subprocess.TimeoutExpired: process.kill()
    log.close()
