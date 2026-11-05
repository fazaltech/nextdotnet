"use client";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect,useState } from "react";
import { ChefHat,LogOut,Menu,X } from "lucide-react";
import { clearAuthSession,type LoginResponse } from "@/lib/auth";
import { serviceApi } from "@/lib/service-api";
const navigation=[
  ["orders","Orders","service.use"],["tables","Tables & seating","service.use"],["kitchen","Kitchen","kitchen.use"],
  ["menu","Menu & recipes","menuitems.manage"],["inventory","Ingredient stock","inventory.manage"],
  ["purchases","Purchases","purchases.manage"],["billing","Bills & payments","invoices.manage","payments.manage"],
  ["customers","Customers","customers.manage"],["expenses","Expenses","expenses.manage"],
  ["analytics","Reports & audit","reports.view"],["settings","Users & settings","users.manage","roles.manage","settings.manage"],
];
export function DashboardShell({session,children}:{session:LoginResponse;children:React.ReactNode}) {
  const pathname=usePathname();const router=useRouter();const [open,setOpen]=useState(false);const [permissions,setPermissions]=useState<string[]>([]);
  useEffect(()=>{serviceApi<string[]>("service/permissions").then(setPermissions).catch(()=>setPermissions([]));},[]);
  return <div className="min-h-screen bg-[#f5f3ef] text-stone-800">
    <button className="fixed left-3 top-3 z-30 rounded-lg bg-white p-2 lg:hidden" aria-label="Open navigation" onClick={()=>setOpen(true)}><Menu/></button>
    <aside className={`fixed inset-y-0 left-0 z-40 flex w-60 flex-col bg-[#1f1e1b] p-5 text-white ${open?"":"hidden lg:flex"}`}>
      <div className="flex items-center justify-between"><Link href="/dashboard" className="flex items-center gap-2 text-xl font-semibold"><ChefHat className="text-[#e76535]"/>Savorly</Link><button className="lg:hidden" aria-label="Close navigation" onClick={()=>setOpen(false)}><X/></button></div>
      <p className="mt-2 text-xs uppercase tracking-widest text-stone-400">Restaurant management</p>
      <nav className="mt-8 flex-1 space-y-1 overflow-auto"><Link onClick={()=>setOpen(false)} href="/dashboard" className="block rounded-lg px-3 py-2 text-sm hover:bg-white/10">Overview</Link>{navigation.filter(([, ,...required])=>required.some(p=>permissions.includes(p))).map(([key,title])=><Link key={key} href={`/dashboard/${key}`} onClick={()=>setOpen(false)} className={`block rounded-lg px-3 py-2 text-sm ${pathname===`/dashboard/${key}`?"bg-[#f5f0e8] text-stone-900":"text-stone-300 hover:bg-white/10"}`}>{title}</Link>)}</nav>
      <p className="truncate text-sm">{session.fullName}</p><p className="mb-4 text-xs text-stone-400">{session.roles.join(", ")}</p><button onClick={()=>{clearAuthSession();router.replace("/");}} className="flex items-center gap-2 text-sm"><LogOut size={16}/>Sign out</button>
    </aside>
    <main className="px-4 pb-8 pt-16 lg:ml-60 lg:p-8">{children}</main>
  </div>;
}
