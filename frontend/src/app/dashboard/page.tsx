"use client";
import Link from "next/link";
import { useEffect,useState } from "react";
import { serviceApi } from "@/lib/service-api";
export default function DashboardPage(){
  const [permissions,setPermissions]=useState<string[]>([]);const [error,setError]=useState("");
  useEffect(()=>{serviceApi<string[]>("service/permissions").then(setPermissions).catch(e=>setError(e.message));},[]);
  const links=[
    ["tables","Open a dining session","Assign a party to tables and seats before taking orders.","service.use"],
    ["orders","Take and deliver orders","Submit items, track manager approval, and deliver ready food.","service.use"],
    ["kitchen","Prepare approved items","Update individual items as preparation progresses.","kitchen.use"],
    ["billing","Settle the party's bill","Split by guest, seat, or selected items; record payments and print receipts.","invoices.manage"],
    ["inventory","Manage ingredients","Maintain recipes, counts, waste, and stock movement history.","inventory.manage"],
    ["purchases","Receive purchases","Record suppliers and purchases; receive goods partially or in full.","purchases.manage"],
    ["analytics","Review business performance","Review sales, ingredient costs, expenses, and profit.","reports.view"],
    ["settings","Manage your team","Create users, reset passwords, and assign permissions.","users.manage"],
  ];
  return <div><h1 className="text-3xl font-semibold">Restaurant overview</h1><p className="mt-2 text-stone-600">Choose a workflow to work with current restaurant data.</p>{error&&<p role="alert" className="mt-4 text-red-700">{error}</p>}<div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{links.filter(x=>permissions.includes(x[3])).map(([route,title,description])=><Link href={`/dashboard/${route}`} key={route} className="rounded-xl border bg-white p-6 hover:border-[#315e4f]"><h2 className="text-lg font-medium">{title}</h2><p className="mt-2 text-sm text-stone-500">{description}</p></Link>)}</div></div>;
}
