"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { HubConnectionBuilder, HttpTransportType, LogLevel } from "@microsoft/signalr";
import { loadAuthSession } from "@/lib/auth";
import { API_BASE_URL } from "@/lib/config";
import { serviceApi } from "@/lib/service-api";

type Row = Record<string, string | number | boolean | null>;
type Source = { key: string; title: string; path: string; permission: string };
type Field = { name: string; label: string; type?: string; source?: string; choices?: string[]; optional?: boolean; multiple?: boolean };
type Action = { title: string; action: string; permission: string; fields: Field[]; endpoint?: string; method?: string };
const f = (name: string, label: string, type = "number", source?: string, optional = false): Field => ({ name, label, type, source, optional });
const reason = f("reason", "Reason / instructions", "text");
const sources: Source[] = [
  { key:"tables",title:"Tables & sessions",path:"service/tables",permission:"service.use" },
  { key:"seats",title:"Seat assignments",path:"service/seats",permission:"service.use" },
  { key:"orders",title:"Orders",path:"service/orders",permission:"service.use" },
  { key:"items",title:"Order items",path:"service/items",permission:"service.use" },
  { key:"kitchen",title:"Kitchen queue",path:"service/kitchen",permission:"kitchen.use" },
  { key:"menu",title:"Menu",path:"menu-items",permission:"lookup" },
  { key:"categories",title:"Categories",path:"food-categories",permission:"lookup" },
  { key:"customers",title:"Customers",path:"customers",permission:"lookup" },
  { key:"recipes",title:"Recipes (stock units)",path:"service/recipes",permission:"inventory.manage" },
  { key:"stock",title:"Ingredients",path:"service/stock",permission:"inventory.manage" },
  { key:"movements",title:"Stock movements",path:"service/movements",permission:"inventory.manage" },
  { key:"suppliers",title:"Suppliers",path:"service/suppliers",permission:"purchases.manage" },
  { key:"purchases",title:"Purchases",path:"service/purchases",permission:"purchases.manage" },
  { key:"purchase-items",title:"Purchase lines / receipts",path:"service/purchase-items",permission:"purchases.manage" },
  { key:"bills",title:"Bills & balances",path:"service/bills",permission:"invoices.manage" },
  { key:"receipt-items",title:"Receipt lines",path:"service/receipt-items",permission:"invoices.manage" },
  { key:"methods",title:"Payment methods",path:"service/payment-methods",permission:"payments.manage" },
  { key:"payments",title:"Payments / refunds",path:"payments",permission:"payments.manage" },
  { key:"expenses",title:"Operating expenses",path:"expenses",permission:"expenses.manage" },
  { key:"users",title:"Users",path:"users",permission:"users.manage" },
  { key:"roles",title:"Roles",path:"roles",permission:"roles.manage" },
  { key:"permissions",title:"Permissions",path:"roles/permissions",permission:"roles.manage" },
  { key:"settings",title:"Settings",path:"settings",permission:"settings.manage" },
  ...["sales","sales-items","payment-report","recipe-cost","profit","audit"].map(key=>({key,title:key.replaceAll("-"," "),path:`service/${key}`,permission:"reports.view"})),
];
const actions: Action[] = [
  { title:"Open table session",action:"open-session",permission:"service.use",fields:[f("tableId","Table","number","tables")] },
  { title:"Assign customer to seat",action:"assign-seat",permission:"service.use",fields:[f("sessionId","Session","number","sessions"),f("tableId","Table","number","tables"),f("customerId","Customer","number","customers"),f("seat","Seat number")] },
  { title:"Remove seat assignment",action:"remove-seat",permission:"service.use",fields:[f("id","Assignment","number","seats")] },
  { title:"Close session",action:"close-session",permission:"service.use",fields:[f("sessionId","Session","number","sessions"),{...reason,optional:true}] },
  { title:"Set table status",action:"table-status",permission:"tables.manage",fields:[f("id","Table","number","tables"),{name:"status",label:"Status",choices:["Available","Reserved","Cleaning"]}] },
  { title:"Move party to table",action:"move-table",permission:"tables.manage",fields:[f("sessionId","Session","number","sessions"),f("targetId","Destination table","number","tables"),reason] },
  { title:"Combine dining sessions",action:"combine-session",permission:"tables.manage",fields:[f("sessionId","Source session","number","sessions"),f("targetId","Destination session","number","sessions"),reason] },
  { title:"Adopt legacy pending order",action:"adopt-order",permission:"orders.manage",fields:[f("id","Legacy order","number","orders"),reason] },
  { title:"Move order to another session",action:"move-order",permission:"orders.manage",fields:[f("id","Order","number","orders"),f("sessionId","Destination session","number","sessions"),f("tableId","Destination table","number","tables"),reason] },
  { title:"Approve order & consume ingredients",action:"approve-order",permission:"orders.manage",fields:[f("id","Order","number","orders")] },
  { title:"Reject submitted order",action:"reject-order",permission:"orders.manage",fields:[f("id","Order","number","orders"),reason] },
  { title:"Edit submitted item quantity",action:"edit-item",permission:"orders.manage",fields:[f("id","Item","number","items"),f("quantity","Quantity"),reason] },
  { title:"Cancel item",action:"cancel-item",permission:"orders.manage",fields:[f("id","Item","number","items"),reason] },
  { title:"Update kitchen preparation",action:"item-status",permission:"kitchen.use",fields:[f("id","Kitchen item","number","kitchen"),{name:"status",label:"Next status",choices:["Preparing","Ready"]}] },
  { title:"Mark ready item served",action:"serve",permission:"service.use",fields:[f("id","Ready item","number","ready")] },
  { title:"Save ingredient recipe",action:"recipe",permission:"inventory.manage",fields:[f("menuItemId","Menu item","number","menu"),f("inventoryItemId","Ingredient","number","stock"),f("quantity","Quantity per menu item (ingredient stock unit)")] },
  { title:"Remove recipe ingredient",action:"remove-recipe",permission:"inventory.manage",fields:[f("menuItemId","Menu item","number","menu"),f("inventoryItemId","Ingredient","number","stock"),reason] },
  { title:"Record physical stock count",action:"stock-count",permission:"inventory.manage",fields:[f("inventoryItemId","Ingredient","number","stock"),f("quantity","Actual quantity counted"),reason] },
  { title:"Adjust stock",action:"stock-adjust",permission:"inventory.manage",fields:[f("inventoryItemId","Ingredient","number","stock"),f("quantity","Change in stock (+ / −)"),reason] },
  { title:"Record waste / spoilage",action:"waste",permission:"inventory.manage",fields:[f("inventoryItemId","Ingredient","number","stock"),f("quantity","Quantity wasted"),reason] },
  { title:"Add supplier",action:"supplier",permission:"purchases.manage",fields:[f("name","Supplier name","text"),f("reference","Contact","text",undefined,true)] },
  { title:"Create purchase order",action:"purchase",permission:"purchases.manage",fields:[f("targetId","Supplier","number","suppliers"),f("reference","Invoice / reference","text"),f("date","Purchase date","date"),f("discount","Discount"),f("tax","Tax")] },
  { title:"Add purchase line",action:"purchase-item",permission:"purchases.manage",fields:[f("id","Purchase","number","purchases"),f("inventoryItemId","Ingredient","number","stock"),f("quantity","Ordered quantity"),f("unitCost","Unit cost")] },
  { title:"Correct unreceived purchase line",action:"correct-purchase-item",permission:"purchases.manage",fields:[f("id","Purchase line","number","purchase-items"),f("quantity","Correct quantity"),f("unitCost","Correct unit cost"),reason] },
  { title:"Receive purchased goods",action:"receive",permission:"purchases.manage",fields:[f("id","Purchase line","number","purchase-items"),f("quantity","Quantity received"),f("reason","Receipt note","text",undefined,true)] },
  { title:"Record supplier payment",action:"purchase-payment",permission:"purchases.manage",fields:[f("id","Purchase","number","purchases"),f("amount","Amount"),f("reason","Payment reference","text")] },
  { title:"Record bill payment",action:"pay",permission:"payments.manage",fields:[f("id","Bill","number","bills"),f("amount","Amount"),{name:"method",label:"Payment method",source:"methods",type:"number"},f("reference","Transaction reference","text",undefined,true)] },
  { title:"Refund payment",action:"refund",permission:"refunds.manage",fields:[f("id","Bill","number","bills"),f("amount","Refund amount"),{name:"method",label:"Refund method",source:"methods",type:"number"},reason] },
  { title:"Void bill after refund",action:"void-bill",permission:"refunds.manage",fields:[f("id","Bill","number","bills"),reason] },
];
const catalog: Record<string,{permission:string;endpoint:string;fields:Field[]}>={
  menu:{permission:"menuitems.manage",endpoint:"menu-items",fields:[f("categoryId","Category","number","categories"),f("name","Name","text"),f("description","Description","text",undefined,true),f("price","Price"),f("isAvailable","Available","checkbox"),f("isActive","Active","checkbox")]},
  categories:{permission:"foodcategories.manage",endpoint:"food-categories",fields:[f("name","Name","text"),f("description","Description","text",undefined,true),f("isActive","Active","checkbox")]},
  tables:{permission:"tables.manage",endpoint:"tables",fields:[f("tableNumber","Table number","text"),f("capacity","Capacity"),f("isOccupied","Occupied (managed by service)","checkbox"),f("isActive","Active","checkbox")]},
  customers:{permission:"customers.manage",endpoint:"customers",fields:[f("fullName","Full name","text"),f("phone","Phone","text",undefined,true),f("email","Email","email",undefined,true),f("isActive","Active","checkbox")]},
  stock:{permission:"inventory.manage",endpoint:"inventory/items",fields:[f("name","Ingredient","text"),f("unit","Stock unit (e.g. kg, litre, each)","text"),f("quantity","Stock quantity (use stock count to change)"),f("reorderLevel","Low-stock threshold"),f("unitCost","Unit cost"),f("isActive","Active","checkbox")]},
  expenses:{permission:"expenses.manage",endpoint:"expenses",fields:[f("title","Title","text"),f("category","Category","text"),f("amount","Amount"),f("expenseDateUtc","Expense date","date"),f("notes","Notes","text",undefined,true),f("isActive","Active","checkbox")]},
  settings:{permission:"settings.manage",endpoint:"settings",fields:[f("settingKey","Key (TaxPercent, ServiceChargePercent, DiscountPercent, PaymentMethods)","text"),f("settingValue","Value","text"),f("description","Description","text",undefined,true),f("isActive","Active","checkbox")]},
};
const userActions:Action[]=[
  {title:"Create user",action:"create-user",permission:"users.manage",endpoint:"users",fields:[f("userName","Username","text"),f("email","Email","email"),f("fullName","Full name","text"),f("password","Initial password","password"),{name:"roles",label:"Roles",source:"roleNames",multiple:true}]},
  {title:"Reset user password",action:"reset-password",permission:"users.manage",endpoint:"users/{id}/reset-password",fields:[f("id","User","number","users"),f("password","New password","password")]},
  {title:"Edit user",action:"edit-user",permission:"users.manage",endpoint:"users/{id}",method:"PUT",fields:[f("id","User","number","users"),f("fullName","Full name","text"),f("email","Email","email"),f("isActive","Active","checkbox")]},
  {title:"Assign user roles",action:"assign-roles",permission:"users.manage",endpoint:"users/{id}/roles",method:"PUT",fields:[f("id","User","number","users"),{name:"roles",label:"Roles",source:"roleNames",multiple:true}]},
  {title:"Create role",action:"create-role",permission:"roles.manage",endpoint:"roles",fields:[f("name","Role name","text"),f("description","Description","text")]},
  {title:"Set role permissions",action:"role-permissions",permission:"roles.manage",endpoint:"roles/{id}/permissions",method:"PUT",fields:[f("id","Role","number","roles"),{name:"permissionIds",label:"Permissions (replaces current grants)",source:"permissions",type:"number",multiple:true,optional:true}]},
];
const groups: Record<string,string[]>={
  orders:["orders","items"],tables:["tables","seats"],kitchen:["kitchen"],menu:["menu","categories","recipes"],inventory:["stock","movements","recipes"],purchases:["suppliers","purchases","purchase-items"],billing:["bills","receipt-items","payments"],customers:["customers"],settings:["users","roles","permissions","settings"],expenses:["expenses"],analytics:["sales","sales-items","payment-report","recipe-cost","profit","audit"],
};
function normalize(rows:Row[]):Row[]{return rows.map(row=>Object.fromEntries(Object.entries(row).map(([key,value])=>[key[0].toLowerCase()+key.slice(1),value])));}
function displayCell(section:string,column:string,value:Row[string]){
  if(column==="status"){
    const names=section==="bills"?["","Unpaid","Partially paid","Paid","Voided","Refunded"]:section==="orders"?["","Submitted","Approved / preparing","Ready","Served","Cancelled"]:section==="payments"?["","Pending","Completed","Failed","Refunded"]:[];
    return names[Number(value)]??String(value??"?");
  }
  if(column==="method")return ["","Cash","Card","Bank transfer","Wallet"][Number(value)]??String(value);
  return String(value??"?");
}
function label(row:Row){return `#${row.id ?? row.sessionId} · ${row.name ?? row.fullName ?? row.tableNumber ?? row.invoiceNumber ?? row.reference ?? row.title ?? row.settingKey ?? ""}${row.status ? ` · status ${row.status}` : ""}${row.preparationStatus ? ` · ${row.preparationStatus}` : ""}`;}
const control="w-full rounded-lg border border-stone-300 bg-white px-3 py-2 text-sm text-stone-900";
const button="rounded-lg bg-[#315e4f] px-4 py-2 text-sm font-medium text-white disabled:opacity-40";

export default function ServiceWorkspace({area="orders"}:{area?:string}) {
  const [permissions,setPermissions]=useState<string[]|null>(null);
  const [data,setData]=useState<Record<string,Row[]>>({});
  const [error,setError]=useState(""); const [notice,setNotice]=useState("");
  const [busy,setBusy]=useState(false); const [loading,setLoading]=useState(true);
  const [connected,setConnected]=useState(false);
  const [from,setFrom]=useState(()=>new Date(Date.now()-30*86400000).toISOString().slice(0,10));
  const [to,setTo]=useState(()=>new Date(Date.now()+86400000).toISOString().slice(0,10));
  const [filter,setFilter]=useState("");
  const [section,setSection]=useState(groups[area]?.[0]??"orders");
  const [formAction,setFormAction]=useState<Action|null>(null);
  const [values,setValues]=useState<Record<string,unknown>>({});
  const [cart,setCart]=useState<Record<string,unknown>[]>([]);
  const [selected,setSelected]=useState<number[]>([]);
  const [billSession,setBillSession]=useState("");
  const [split,setSplit]=useState("");
  const requestKey=useRef<string|null>(null);
  const can=useCallback((p:string)=>!!permissions?.includes(p),[permissions]);
  const permitted=useCallback((s:Source)=>s.permission==="lookup" ? can("service.use")||can(s.key==="menu"?"menuitems.manage":s.key==="categories"?"foodcategories.manage":"customers.manage") : (can(s.permission)||(["bills","receipt-items"].includes(s.key)&&can("payments.manage"))),[can]);
  const reload=useCallback(async()=>{
    if(!permissions)return;
    const results=await Promise.allSettled(sources.filter(permitted).map(async s=>[s.key,normalize(await serviceApi<Row[]>(s.path+(s.path.startsWith("service/")?`?from=${from}&to=${to}`:"")))] as const));
    const entries: [string,Row[]][]=[]; const errors:string[]=[];
    for(const result of results)if(result.status==="fulfilled") entries.push([...result.value]); else errors.push(result.reason instanceof Error?result.reason.message:"Could not load data.");
    setData(Object.fromEntries(entries));setLoading(false);
    if(errors.length)setError([...new Set(errors)].join(" "));
  },[permissions,permitted,from,to]);
  useEffect(()=>{serviceApi<string[]>("service/permissions").then(setPermissions).catch(e=>{setError(e.message);setLoading(false);});},[]);
  useEffect(()=>{void reload();},[reload]);
  useEffect(()=>{
    if(!permissions)return;
    const hub=new HubConnectionBuilder().withUrl(API_BASE_URL.replace(/\/api$/,"")+"/hubs/restaurant",{accessTokenFactory:()=>loadAuthSession()?.accessToken??"",transport:HttpTransportType.WebSockets,skipNegotiation:true}).withAutomaticReconnect().configureLogging(LogLevel.Error).build();
    let disposed=false;
    hub.on("Changed",()=>{if(!disposed)void reload();});
    hub.onreconnecting(()=>{if(!disposed)setConnected(false);});
    hub.onreconnected(()=>{if(!disposed){setConnected(true);void reload();}});
    hub.onclose(()=>{if(!disposed)setConnected(false);});
    // Skip startup if React cleans up this effect before the next microtask.
    const started=Promise.resolve().then(async()=>{
      if(disposed)return;
      await hub.start();
      if(!disposed){setConnected(true);void reload();}
    }).catch(()=>{if(!disposed)setConnected(false);});
    const poll=window.setInterval(()=>{void reload();},30000);
    return()=>{
      disposed=true;
      window.clearInterval(poll);
      // SignalR logs an error if stop interrupts a pending start.
      void started.then(()=>hub.stop()).catch(error=>console.error("Failed to stop restaurant connection.",error));
    };
  },[permissions,reload]);
  const lists=useMemo<Record<string,Row[]>>(()=>({...data,
    sessions:(data.tables??[]).filter(r=>r.sessionId).map(r=>({...r,id:r.sessionId})),
    ready:(data.items??[]).filter(r=>r.preparationStatus==="Ready"),
    roleNames:(data.roles??[]).map(r=>({...r,id:r.name})),
    methods:data.methods??[],
  }),[data]);
  async function mutate(path:string,body:Record<string,unknown>,method="POST") {
    setBusy(true);setError("");setNotice("");
    try {
      requestKey.current??=crypto.randomUUID();
      const result=await serviceApi<{id?:number}|undefined>(path,method,path.startsWith("service/")?{...body,requestKey:requestKey.current}:body);
      requestKey.current=null;setNotice(`Saved${result?.id?` (#${result.id})`:""}.`);setFormAction(null);setValues({});setCart([]);setSelected([]);await reload();
    } catch(e){setError(e instanceof Error?e.message:"Unable to save.");} finally{setBusy(false);}
  }
  function editValue(name:string,value:unknown){requestKey.current=null;setValues(v=>({...v,[name]:value}));}
  function renderField(field:Field){
    const value=values[field.name];const options=field.source?lists[field.source]??[]:field.choices?.map(x=>({id:x,name:x}));
    return <label className="grid gap-1 text-sm" key={field.name}>{field.label}
      {options?<select className={control} required={!field.optional} multiple={field.multiple} value={field.multiple?(value as string[]??[]):String(value??"")} onChange={e=>editValue(field.name,field.multiple?Array.from(e.target.selectedOptions,x=>field.type==="number"?Number(x.value):x.value):field.type==="number"?Number(e.target.value):e.target.value)}>
        {!field.multiple&&<option value="">Select…</option>}{options.map((row,index)=><option key={`${row.id}-${index}`} value={String(row.id)}>{label(row)}</option>)}
      </select>:<input className={field.type==="checkbox"?"h-5 w-5":control} type={field.type??"text"} step={field.type==="number"?"any":undefined} maxLength={field.type==="text"?1000:undefined} required={!field.optional&&field.type!=="checkbox"} checked={field.type==="checkbox"?!!value:undefined} value={field.type==="checkbox"?undefined:String(value??"")} onChange={e=>editValue(field.name,field.type==="checkbox"?e.target.checked:field.type==="number"?Number(e.target.value):e.target.value)}/>}
    </label>;
  }
  function openAction(action:Action,initial:Record<string,unknown>={}){requestKey.current=null;setFormAction(action);setValues({...Object.fromEntries(action.fields.filter(f=>f.multiple).map(f=>[f.name,[]])),...initial});setError("");setNotice("");}
  const visible=sources.filter(s=>groups[area]?.includes(s.key)&&permitted(s));
  const availableActions=[...actions,...userActions].filter(a=>can(a.permission)&&(
    area==="tables"?["open-session","assign-seat","remove-seat","close-session","table-status","move-table","combine-session"].includes(a.action):
    area==="orders"?["adopt-order","move-order","approve-order","reject-order","edit-item","cancel-item","serve"].includes(a.action):
    area==="kitchen"?a.action==="item-status":
    area==="menu"?["recipe","remove-recipe"].includes(a.action):
    area==="inventory"?["recipe","remove-recipe","stock-adjust","stock-count","waste"].includes(a.action):
    area==="purchases"?["supplier","purchase","purchase-item","receive","purchase-payment","correct-purchase-item"].includes(a.action):
    area==="billing"?["pay","refund","void-bill"].includes(a.action):area==="settings"?userActions.includes(a):false));
  const activeSection=visible.some(s=>s.key===section)?section:visible[0]?.key??section;
  const rows=(data[activeSection]??[]).filter(row=>!filter||Object.values(row).some(value=>String(value??"").toLowerCase().includes(filter.toLowerCase())));
  const columns=Array.from(new Set(rows.flatMap(Object.keys))).filter(k=>!["isActive","updatedAtUtc"].includes(k));
  const crud=catalog[activeSection];
  const served=(data.items??[]).filter(r=>r.sessionId===Number(billSession)&&r.preparationStatus==="Served"&&!(data["receipt-items"]??[]).some(a=>a.id===r.id&&(data.bills??[]).some(b=>b.id===a.invoiceId&&b.status!==4)));
  const splitOptions=Array.from(new Set(served.flatMap(r=>[...(r.customerId?[`customer:${r.customerId}`]:[]),...(r.seat?[`seat:${r.tableId}:${r.seat}`]:[])])));
  function printReceipt(bill:Row){
    const win=window.open("","_blank");if(!win){setError("Allow popups to print receipts.");return;}
    const h=win.document.createElement("h1");h.textContent=String(bill.invoiceNumber);win.document.body.append(h);
    const pre=win.document.createElement("pre");pre.textContent=[`Date: ${bill.invoiceDateUtc}`,...(data["receipt-items"]??[]).filter(i=>i.invoiceId===bill.id).map(i=>`${i.quantity} × ${i.name}  ${i.lineTotal}`),`Subtotal: ${bill.subtotal??bill.totalAmount}`,`Discount: ${bill.discount}`,`Tax: ${bill.taxAmount}`,`Service: ${bill.serviceCharge}`,`Total: ${bill.totalAmount}`,`Refunded: ${bill.refundAmount}`,`Net paid: ${bill.paidAmount}`,`Balance: ${bill.balance}`].join("\n");win.document.body.append(pre);win.print();
  }
  return <div className="space-y-5 text-stone-800">
    <div className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-semibold capitalize">{area==="analytics"?"Business reports":area}</h1><p className="text-sm text-stone-500">{connected?"Live updates connected":"Refreshing every 30 seconds"} · All values come from the restaurant database.</p></div><button className={button} onClick={()=>{setError("");void reload();}} disabled={busy}>Refresh</button></div>
    {error&&<p role="alert" className="rounded-lg bg-red-50 p-3 text-red-800">{error}</p>}{notice&&<p role="status" className="rounded-lg bg-green-50 p-3 text-green-800">{notice}</p>}
    {loading&&<p role="status">Loading restaurant data…</p>}
    {(data.stock??[]).some(item=>Number(item.quantity)<=Number(item.reorderLevel))&&<p role="status" className="rounded-lg bg-amber-50 p-3 text-sm text-amber-900">Low stock: {(data.stock??[]).filter(item=>Number(item.quantity)<=Number(item.reorderLevel)).map(item=>`${item.name} (${item.quantity} ${item.unit})`).join(", ")}. Review ingredient stock before approving orders.</p>}
    {area==="analytics"&&<p className="text-sm">Dates use UTC; end date is exclusive. Gross profit = sales after discounts − recorded ingredient cost. Net profit also deducts operating expenses. Historical orders without recipe consumption have no recorded ingredient cost. Recipe cost is an estimate at current ingredient prices.</p>}
    {["analytics","inventory","purchases"].includes(area)&&<div className="flex gap-3"><label>From<input className={control} type="date" value={from} onChange={e=>setFrom(e.target.value)}/></label><label>Until (exclusive)<input className={control} type="date" value={to} onChange={e=>setTo(e.target.value)}/></label></div>}
    <div className="flex flex-wrap gap-2">{availableActions.map(a=><button className="rounded-lg border border-stone-300 bg-white px-3 py-2 text-sm" key={a.title} onClick={()=>openAction(a)} disabled={busy}>{a.title}</button>)}</div>
    {formAction&&<form className="grid gap-3 rounded-xl border bg-white p-5 sm:grid-cols-2" onSubmit={e=>{e.preventDefault();const path=formAction.endpoint?.replace("{id}",String(values.id))??`service/${formAction.action==="serve"?"item-status":formAction.action}`;void mutate(path,formAction.action==="serve"?{...values,status:"Served"}:values,formAction.method??"POST");}}>
      <h2 className="text-lg font-medium sm:col-span-2">{formAction.title}</h2>{formAction.fields.map(renderField)}<div className="flex gap-2 sm:col-span-2"><button className={button} disabled={busy}>Save</button><button type="button" onClick={()=>setFormAction(null)}>Cancel</button></div>
    </form>}
    {area==="orders"&&can("service.use")&&<form className="space-y-3 rounded-xl border bg-white p-5" onSubmit={e=>{e.preventDefault();if(!cart.length){setError("Add at least one item.");return;}void mutate("service/submit-order",{sessionId:Number(values.sessionId),tableId:Number(values.tableId),items:cart});}}>
      <h2 className="text-lg font-medium">Submit a table order</h2><div className="grid gap-3 sm:grid-cols-3">{[f("sessionId","Dining session","number","sessions"),f("tableId","Table","number","tables"),f("menuItemId","Menu item","number","menu"),f("quantity","Quantity"),f("customerId","Customer (optional)","number","customers",true),f("seat","Seat (optional)","number",undefined,true),f("instructions","Options / special instructions","text",undefined,true)].map(renderField)}</div>
      <button type="button" className={button} disabled={busy} onClick={()=>{if(!values.menuItemId||!(Number(values.quantity)>0)){setError("Choose a menu item and positive quantity.");return;}requestKey.current=null;setCart(c=>[...c,{menuItemId:Number(values.menuItemId),quantity:Number(values.quantity),customerId:values.customerId||null,seat:values.seat||null,instructions:values.instructions||null}]);}}>Add item</button>
      <ul>{cart.map((item,index)=><li key={index} className="flex gap-3 py-1">{String(item.quantity)} × {String((data.menu??[]).find(m=>m.id===item.menuItemId)?.name)} · seat {String(item.seat??"party")} · {String(item.instructions??"")}<button type="button" onClick={()=>{requestKey.current=null;setCart(c=>c.filter((_,i)=>i!==index));}}>Remove</button></li>)}</ul><button className={button} disabled={busy||!cart.length}>Submit for manager approval</button>
    </form>}
    {area==="billing"&&can("invoices.manage")&&<div className="space-y-3 rounded-xl border bg-white p-5"><h2 className="text-lg font-medium">Create party or split bill</h2><p className="text-sm">Select served items. Prices, discounts, tax, and service charges are calculated by the server. Each line is billed once.</p>
      <select aria-label="Dining session" className={control} value={billSession} onChange={e=>{setBillSession(e.target.value);setSelected([]);setSplit("");requestKey.current=null;}}><option value="">Select session…</option>{lists.sessions.map((s,i)=><option key={i} value={String(s.id)}>{label(s)}</option>)}</select>
      <select aria-label="Split bill by customer or seat" className={control} value={split} onChange={e=>{const v=e.target.value;setSplit(v);requestKey.current=null;setSelected(served.filter(r=>!v||(v.startsWith("customer:")?r.customerId===Number(v.split(":")[1]):r.tableId===Number(v.split(":")[1])&&r.seat===Number(v.split(":")[2]))).map(r=>Number(r.id)));}}><option value="">Whole party / selected items</option>{splitOptions.map(s=><option key={s}>{s}</option>)}</select>
      <button className="text-sm underline" onClick={()=>{requestKey.current=null;setSelected(served.map(i=>Number(i.id)));}}>Select all unbilled items</button>
      {served.map(i=><label key={String(i.id)} className="flex gap-2"><input type="checkbox" checked={selected.includes(Number(i.id))} onChange={e=>{requestKey.current=null;setSelected(s=>e.target.checked?[...s,Number(i.id)]:s.filter(x=>x!==i.id));}}/>{label(i)} × {i.quantity} · {i.lineTotal}</label>)}
      <button className={button} disabled={busy||!selected.length} onClick={()=>void mutate("service/bill",{sessionId:Number(billSession),itemIds:selected})}>Generate bill</button>
    </div>}
    <div className="flex flex-wrap gap-2">{visible.map(s=><button key={s.key} onClick={()=>setSection(s.key)} className={`rounded-lg px-4 py-2 text-sm ${activeSection===s.key?"bg-stone-800 text-white":"bg-white"}`}>{s.title}</button>)}</div>
    {crud&&can(crud.permission)&&<button className={button} disabled={busy} onClick={()=>openAction({title:`Add ${activeSection}`,action:"create",permission:crud.permission,endpoint:crud.endpoint,fields:crud.fields},{isActive:true,isAvailable:true,isOccupied:false,quantity:0})}>Add {activeSection}</button>}
    {!loading&&!visible.length&&<p>Your role has no access to this section. Choose an authorized section from the navigation.</p>}
    {visible.length>0&&<label className="block text-sm">Filter records (item, supplier, table, employee, status)<input className={control} value={filter} onChange={e=>setFilter(e.target.value)}/></label>}
    {visible.length>0&&<div className="overflow-x-auto rounded-xl border bg-white"><table className="w-full whitespace-nowrap text-left text-sm"><thead className="bg-stone-100"><tr>{columns.map(c=><th key={c} className="px-3 py-3 font-medium">{c.replace(/([A-Z])/g," $1")}</th>)}{(crud&&can(crud.permission)||activeSection==="bills")&&<th className="px-3">Actions</th>}</tr></thead><tbody>{rows.map((row,index)=><tr key={index} className={`border-t ${activeSection==="stock"&&Number(row.quantity)<=Number(row.reorderLevel)?"bg-amber-50":""}`}>{columns.map(c=><td key={c} className="max-w-md overflow-hidden text-ellipsis px-3 py-3" title={String(row[c]??"")}>{Array.isArray(row[c])?(row[c] as unknown as string[]).join(", "):typeof row[c]==="boolean"?(row[c]?"Yes":"No"):displayCell(activeSection,c,row[c])}</td>)}{crud&&can(crud.permission)&&<td className="px-3"><button className="underline" onClick={()=>openAction({title:`Edit ${activeSection} #${row.id}`,action:"update",permission:crud.permission,endpoint:`${crud.endpoint}/${row.id}`,method:"PUT",fields:crud.fields},Object.fromEntries(Object.entries(row).map(([k,v])=>[k,k.toLowerCase().includes("date")&&typeof v==="string"?v.slice(0,10):v])))}>Edit</button></td>}{activeSection==="bills"&&<td className="px-3"><button className="underline" onClick={()=>printReceipt(row)}>Print / reprint</button></td>}</tr>)}</tbody></table>{!rows.length&&!loading&&<p className="p-5 text-stone-500">No records in this section or date range.</p>}</div>}
  </div>;
}
