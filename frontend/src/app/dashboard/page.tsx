"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import {
  ArrowDownRight,
  ArrowUpRight,
  Banknote,
  Boxes,
  ChefHat,
  CircleDollarSign,
  Clock3,
  MoreHorizontal,
  ReceiptText,
  ShoppingBag,
  Sparkles,
  TrendingUp,
  UsersRound,
  Utensils,
} from "lucide-react";
import { apiGet } from "@/lib/api";
import {
  dashboardFallback,
  formatCurrency,
  getOrderStatus,
  inventoryFallback,
  menuFallback,
  minutesAgo,
  orderFallback,
  tableFallback,
  weeklySales,
  type DashboardSummary,
  type DiningTable,
  type InventoryItem,
  type MenuItem,
  type RestaurantOrder,
} from "@/lib/restaurant-data";
import { OrderStatusBadge, SectionTitle, TextLink } from "@/components/dashboard/ui";

function SalesChart() {
  const max = Math.max(...weeklySales.map((item) => item.sales));
  const points = weeklySales.map((item, index) => {
    const x = 4 + index * 16;
    const y = 86 - (item.sales / max) * 68;
    return `${x},${y}`;
  }).join(" ");
  const area = `4,92 ${points} 100,92`;

  return (
    <div className="mt-5">
      <div className="relative h-44 w-full">
        <div className="absolute inset-0 flex flex-col justify-between text-[0.58rem] text-[#a8a198]">
          {["$6k", "$4k", "$2k", "$0"].map((label) => <div key={label} className="flex items-center gap-3"><span className="w-6">{label}</span><span className="h-px flex-1 bg-[#eeeae3]" /></div>)}
        </div>
        <svg aria-label="Weekly sales trend" viewBox="0 0 104 96" preserveAspectRatio="none" className="absolute inset-y-0 left-8 h-[calc(100%-0.75rem)] w-[calc(100%-2rem)] overflow-visible">
          <defs>
            <linearGradient id="salesArea" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#e76535" stopOpacity="0.22" />
              <stop offset="100%" stopColor="#e76535" stopOpacity="0" />
            </linearGradient>
          </defs>
          <polygon points={area} fill="url(#salesArea)" />
          <polyline points={points} fill="none" stroke="#e76535" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" vectorEffect="non-scaling-stroke" />
          {weeklySales.map((item, index) => {
            const x = 4 + index * 16;
            const y = 86 - (item.sales / max) * 68;
            return <circle key={item.day} cx={x} cy={y} r="1.8" fill="white" stroke="#e76535" strokeWidth="1.4" vectorEffect="non-scaling-stroke" />;
          })}
        </svg>
      </div>
      <div className="ml-8 grid grid-cols-7 text-center text-[0.6rem] font-medium text-[#918a80]">
        {weeklySales.map((item) => <span key={item.day}>{item.day}</span>)}
      </div>
    </div>
  );
}

export default function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary>(dashboardFallback);
  const [orders, setOrders] = useState<RestaurantOrder[]>(orderFallback);
  const [tables, setTables] = useState<DiningTable[]>(tableFallback);
  const [menu, setMenu] = useState<MenuItem[]>(menuFallback);
  const [inventory, setInventory] = useState<InventoryItem[]>(inventoryFallback);

  useEffect(() => {
    Promise.all([
      apiGet("dashboard/summary", dashboardFallback),
      apiGet("orders", orderFallback),
      apiGet("tables", tableFallback),
      apiGet("menu-items", menuFallback),
      apiGet("inventory/items", inventoryFallback),
    ]).then(([summaryRows, orderRows, tableRows, menuRows, inventoryRows]) => {
      setSummary(summaryRows);
      setOrders(orderRows);
      setTables(tableRows);
      setMenu(menuRows);
      setInventory(inventoryRows);
    }).catch(() => undefined);
  }, []);

  const occupied = tables.filter((table) => table.isOccupied).length;
  const activeOrders = orders.filter((order) => !["Served", "Cancelled"].includes(getOrderStatus(order.status)));
  const lowStock = inventory.filter((item) => item.quantity <= item.reorderLevel).length;
  const averageOrder = orders.length ? orders.reduce((sum, order) => sum + order.totalAmount, 0) / orders.length : 0;
  const popular = useMemo(() => menu.filter((item) => item.isAvailable).slice(0, 4), [menu]);

  const kpis = [
    { label: "Today's revenue", value: formatCurrency(summary.todaySales), note: "+12.8% vs yesterday", icon: CircleDollarSign, trend: "up", accent: "bg-[#e76535] text-white", iconBg: "bg-white/15" },
    { label: "Active orders", value: String(activeOrders.length || summary.pendingOrders), note: `${orders.length} total orders today`, icon: ShoppingBag, trend: "up", accent: "bg-white text-[#292520]", iconBg: "bg-[#fff0e9] text-[#e76535]" },
    { label: "Avg. order value", value: formatCurrency(averageOrder || 48.6), note: "+4.2% this week", icon: ReceiptText, trend: "up", accent: "bg-white text-[#292520]", iconBg: "bg-[#e8f3ee] text-[#37705c]" },
    { label: "Dining room", value: `${occupied}/${tables.filter((table) => table.isActive).length}`, note: `${Math.round((occupied / Math.max(tables.length, 1)) * 100)}% tables occupied`, icon: Utensils, trend: "neutral", accent: "bg-white text-[#292520]", iconBg: "bg-[#f3edfa] text-[#775e98]" },
  ];

  return (
    <div className="space-y-5 lg:space-y-6">
      <section className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-2">
            <span className="flex h-8 w-8 items-center justify-center rounded-xl bg-[#fff0e9] text-[#e76535]"><Sparkles className="h-4 w-4" /></span>
            <p className="text-sm font-semibold text-[#39342e]">Good afternoon, Alex</p>
          </div>
          <p className="mt-2 text-xs leading-5 text-[#8d867c]">Here’s what’s happening across The Ember Table right now.</p>
        </div>
        <div className="flex gap-2">
          <button type="button" className="btn-secondary"><Banknote className="h-4 w-4" />End-of-day</button>
          <Link href="/dashboard/orders?new=1" className="btn-primary"><ShoppingBag className="h-4 w-4" />Create order</Link>
        </div>
      </section>

      <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {kpis.map(({ label, value, note, icon: Icon, trend, accent, iconBg }) => (
          <article key={label} className={`rounded-2xl border p-4 shadow-[0_9px_28px_rgba(60,46,31,0.05)] ${accent.includes("bg-white") ? "border-[#eae6de]" : "border-transparent"} ${accent}`}>
            <div className="flex items-start justify-between gap-4">
              <div>
                <p className={`text-[0.65rem] font-medium ${accent.includes("bg-white") ? "text-[#8e877e]" : "text-white/70"}`}>{label}</p>
                <p className="mt-2 text-[1.65rem] font-semibold tracking-[-0.05em]">{value}</p>
              </div>
              <span className={`flex h-9 w-9 items-center justify-center rounded-xl ${iconBg}`}><Icon className="h-[1.05rem] w-[1.05rem]" /></span>
            </div>
            <div className={`mt-4 flex items-center gap-1.5 text-[0.61rem] ${accent.includes("bg-white") ? "text-[#8e877e]" : "text-white/70"}`}>
              {trend === "up" ? <ArrowUpRight className={`h-3.5 w-3.5 ${accent.includes("bg-white") ? "text-[#3c8367]" : "text-white"}`} /> : <span className="h-1.5 w-1.5 rounded-full bg-[#e9b44c]" />}
              {note}
            </div>
          </article>
        ))}
      </section>

      <section className="grid gap-5 xl:grid-cols-[1.35fr_0.65fr]">
        <article className="surface p-5 sm:p-6">
          <SectionTitle eyebrow="Revenue" title="Sales performance" action={<div className="flex rounded-lg bg-[#f4f1ec] p-1 text-[0.62rem] font-medium text-[#827b72]"><button className="rounded-md px-2.5 py-1.5">Day</button><button className="rounded-md bg-white px-2.5 py-1.5 text-[#37322c] shadow-sm">Week</button><button className="rounded-md px-2.5 py-1.5">Month</button></div>} />
          <div className="mt-4 flex flex-wrap items-end gap-x-5 gap-y-2">
            <p className="text-3xl font-semibold tracking-[-0.055em] text-[#28231f]">{formatCurrency(31_186)}</p>
            <p className="mb-1 flex items-center gap-1 text-[0.66rem] font-semibold text-[#3d8066]"><TrendingUp className="h-3.5 w-3.5" />9.4% from last week</p>
          </div>
          <SalesChart />
        </article>

        <article className="surface p-5 sm:p-6">
          <SectionTitle eyebrow="Live floor" title="Table occupancy" action={<Link href="/dashboard/tables"><TextLink>View map</TextLink></Link>} />
          <div className="mt-5 grid grid-cols-4 gap-2.5">
            {tables.slice(0, 16).map((table) => (
              <div key={table.id} className={`flex aspect-square flex-col items-center justify-center rounded-xl border text-center ${!table.isActive ? "border-dashed border-[#d9d3ca] bg-[#faf9f6] text-[#b0a99f]" : table.isOccupied ? "border-[#f0c8b9] bg-[#fff0e9] text-[#c75329]" : "border-[#dfe9e4] bg-[#eef6f2] text-[#3f735f]"}`}>
                <span className="text-[0.68rem] font-semibold">{table.tableNumber.replace("T-", "")}</span>
                <span className="mt-0.5 text-[0.52rem] opacity-65">{table.capacity} seats</span>
              </div>
            ))}
          </div>
          <div className="mt-4 flex items-center gap-4 text-[0.58rem] text-[#8c857c]">
            <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-[#e76535]" />Occupied</span>
            <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-[#4f8a72]" />Available</span>
            <span className="ml-auto font-semibold text-[#38332d]">{occupied} active</span>
          </div>
        </article>
      </section>

      <section className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr_0.62fr]">
        <article className="surface overflow-hidden">
          <div className="p-5 pb-3 sm:px-6 sm:pt-6"><SectionTitle eyebrow="Kitchen queue" title="Recent orders" action={<Link href="/dashboard/orders"><TextLink>All orders</TextLink></Link>} /></div>
          <div className="divide-y divide-[#eeeae3]">
            {orders.slice(0, 4).map((order, index) => (
              <Link href="/dashboard/orders" key={order.id} className="flex items-center gap-3 px-5 py-3.5 transition-colors hover:bg-[#faf8f4] sm:px-6">
                <span className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${["bg-[#fff0e9] text-[#d45d31]", "bg-[#eaf3ef] text-[#39715d]", "bg-[#eeeafb] text-[#735f9b]", "bg-[#fff5df] text-[#a97720]"][index % 4]}`}><ChefHat className="h-4 w-4" /></span>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2"><p className="text-[0.72rem] font-semibold text-[#35302a]">#{order.id}</p><span className="text-[0.58rem] text-[#aaa39a]">•</span><p className="truncate text-[0.65rem] text-[#777067]">{order.tableNumber}</p></div>
                  <p className="mt-1 truncate text-[0.59rem] text-[#9b948a]">{order.items.map((item) => `${item.quantity}× ${item.menuItemName}`).join(", ")}</p>
                </div>
                <div className="hidden text-right sm:block"><OrderStatusBadge status={order.status} /><p className="mt-1.5 text-[0.55rem] text-[#aaa39a]">{minutesAgo(order.orderDateUtc)}</p></div>
                <p className="w-14 text-right text-[0.72rem] font-semibold text-[#3b362f]">{formatCurrency(order.totalAmount)}</p>
              </Link>
            ))}
          </div>
        </article>

        <article className="surface p-5 sm:p-6">
          <SectionTitle eyebrow="Top sellers" title="Popular dishes" action={<button type="button" className="text-[#999188]"><MoreHorizontal className="h-5 w-5" /></button>} />
          <div className="mt-5 space-y-4">
            {popular.map((item, index) => (
              <div key={item.id} className="flex items-center gap-3">
                <div className={`relative flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-xl ${["bg-[#f3d4b9]", "bg-[#dbe5c7]", "bg-[#f0d9d1]", "bg-[#d8dfeb]"][index]}`}>
                  <span className="absolute h-7 w-7 rounded-full bg-white/35" />
                  <Utensils className="relative h-4 w-4 text-[#4b433a]/60" />
                </div>
                <div className="min-w-0 flex-1"><p className="truncate text-[0.7rem] font-semibold text-[#3b362f]">{item.name}</p><p className="mt-1 text-[0.58rem] text-[#9c958b]">{[42, 38, 31, 27][index]} orders today</p></div>
                <p className="text-[0.7rem] font-semibold text-[#4d4740]">{formatCurrency(item.price)}</p>
              </div>
            ))}
          </div>
        </article>

        <aside className="space-y-5">
          <article className="rounded-3xl bg-[#285f50] p-5 text-white shadow-[0_14px_35px_rgba(40,95,80,0.2)]">
            <div className="flex items-center justify-between"><span className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10"><Clock3 className="h-4 w-4" /></span><span className="rounded-full bg-[#d8b45f] px-2 py-1 text-[0.55rem] font-semibold text-[#362b13]">ON TRACK</span></div>
            <p className="mt-5 text-[0.64rem] text-white/60">Avg. prep time</p>
            <p className="mt-1 text-3xl font-semibold tracking-[-0.05em]">18<span className="ml-1 text-sm font-medium text-white/55">min</span></p>
            <div className="mt-4 h-1.5 overflow-hidden rounded-full bg-white/10"><div className="h-full w-[72%] rounded-full bg-[#e5bc62]" /></div>
            <p className="mt-2 text-[0.58rem] text-white/45">2 min faster than your daily goal</p>
          </article>

          <article className="surface p-5">
            <div className="flex items-center justify-between"><p className="eyebrow">Stock alerts</p><span className="flex h-7 min-w-7 items-center justify-center rounded-lg bg-[#fff0e9] px-1.5 text-[0.62rem] font-semibold text-[#d35b2f]">{lowStock || summary.lowStockItems}</span></div>
            <p className="mt-3 text-sm font-semibold tracking-[-0.025em] text-[#37312c]">Items need attention</p>
            <Link href="/dashboard/inventory" className="mt-4 flex items-center gap-2 text-[0.65rem] font-semibold text-[#e76535]"><Boxes className="h-3.5 w-3.5" />Review inventory <ArrowDownRight className="ml-auto h-3.5 w-3.5 -rotate-90" /></Link>
          </article>
        </aside>
      </section>

      <section className="grid gap-3 sm:grid-cols-3">
        <div className="surface flex items-center gap-3 p-4"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#eaf3ef] text-[#3d765f]"><UsersRound className="h-4 w-4" /></span><div><p className="text-lg font-semibold tracking-[-0.04em]">{summary.totalCustomers.toLocaleString()}</p><p className="text-[0.6rem] text-[#918a80]">Total customers</p></div></div>
        <div className="surface flex items-center gap-3 p-4"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#fff0e9] text-[#d65c30]"><Utensils className="h-4 w-4" /></span><div><p className="text-lg font-semibold tracking-[-0.04em]">{summary.totalMenuItems}</p><p className="text-[0.6rem] text-[#918a80]">Active menu items</p></div></div>
        <div className="surface flex items-center gap-3 p-4"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#f2edfa] text-[#775e99]"><Banknote className="h-4 w-4" /></span><div><p className="text-lg font-semibold tracking-[-0.04em]">{formatCurrency(summary.todayExpenses)}</p><p className="text-[0.6rem] text-[#918a80]">Today’s expenses</p></div></div>
      </section>
    </div>
  );
}
