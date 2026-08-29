"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import {
  Armchair,
  BarChart3,
  Bell,
  Boxes,
  ChefHat,
  ChevronDown,
  CircleHelp,
  Clock3,
  LayoutDashboard,
  LogOut,
  Menu,
  MessageSquareText,
  Plus,
  Search,
  Settings,
  ShoppingBag,
  Sparkles,
  UtensilsCrossed,
  UsersRound,
  X,
  type LucideIcon,
} from "lucide-react";
import { clearAuthSession, type LoginResponse } from "@/lib/auth";

type DashboardShellProps = {
  session: LoginResponse;
  children: React.ReactNode;
};

type NavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
  badge?: string;
};

const primaryNavigation: NavItem[] = [
  { href: "/dashboard", label: "Overview", icon: LayoutDashboard },
  { href: "/dashboard/orders", label: "Orders", icon: ShoppingBag, badge: "12" },
  { href: "/dashboard/menu", label: "Menu", icon: UtensilsCrossed },
  { href: "/dashboard/tables", label: "Tables", icon: Armchair },
  { href: "/dashboard/inventory", label: "Inventory", icon: Boxes, badge: "5" },
  { href: "/dashboard/customers", label: "Customers", icon: UsersRound },
];

const insightNavigation: NavItem[] = [
  { href: "/dashboard/analytics", label: "Analytics", icon: BarChart3 },
  { href: "/dashboard/messages", label: "Messages", icon: MessageSquareText, badge: "3" },
];

const routeTitles: Record<string, { eyebrow: string; title: string }> = {
  "/dashboard": { eyebrow: "Sunday, August 23", title: "Restaurant overview" },
  "/dashboard/orders": { eyebrow: "Live service", title: "Order management" },
  "/dashboard/menu": { eyebrow: "Food & beverage", title: "Menu management" },
  "/dashboard/tables": { eyebrow: "Dining room", title: "Tables & seating" },
  "/dashboard/inventory": { eyebrow: "Stock room", title: "Inventory control" },
  "/dashboard/customers": { eyebrow: "Guest book", title: "Customers" },
  "/dashboard/analytics": { eyebrow: "Business health", title: "Reports & analytics" },
  "/dashboard/messages": { eyebrow: "Team communication", title: "Message center" },
  "/dashboard/settings": { eyebrow: "Workspace", title: "Restaurant settings" },
};

function NavLink({ item, onNavigate }: { item: NavItem; onNavigate: () => void }) {
  const pathname = usePathname();
  const active =
    pathname === item.href ||
    (item.href !== "/dashboard" && pathname.startsWith(`${item.href}/`));
  const Icon = item.icon;

  return (
    <Link
      href={item.href}
      onClick={onNavigate}
      className={`group flex items-center gap-3 rounded-xl px-3 py-2.5 text-[0.8rem] font-medium transition-colors ${
        active
          ? "bg-[#f5f0e8] text-[#28231e]"
          : "text-[#aaa39a] hover:bg-white/6 hover:text-white"
      }`}
    >
      <Icon className={`h-[1.1rem] w-[1.1rem] ${active ? "text-[#e76535]" : "text-[#858078] group-hover:text-white"}`} strokeWidth={1.8} />
      <span className="flex-1">{item.label}</span>
      {item.badge ? (
        <span className={`min-w-5 rounded-md px-1.5 py-0.5 text-center text-[0.62rem] font-semibold ${active ? "bg-[#e76535] text-white" : "bg-white/8 text-[#aaa39a]"}`}>
          {item.badge}
        </span>
      ) : null}
    </Link>
  );
}

function Sidebar({
  session,
  onClose,
}: {
  session: LoginResponse;
  onClose: () => void;
}) {
  const router = useRouter();
  const initials = session.fullName
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");

  return (
    <div className="flex h-full flex-col bg-[#1f1e1b] px-3 pb-4 pt-5 text-white">
      <div className="flex items-center justify-between px-2.5">
        <Link href="/dashboard" className="flex items-center gap-2.5" onClick={onClose}>
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-[#e76535] text-white shadow-[0_7px_20px_rgba(231,101,53,0.28)]">
            <ChefHat className="h-5 w-5" strokeWidth={2} />
          </span>
          <span>
            <span className="block text-[1.05rem] font-semibold tracking-[-0.03em]">Savorly</span>
            <span className="block text-[0.55rem] font-medium uppercase tracking-[0.2em] text-[#817c74]">Restaurant OS</span>
          </span>
        </Link>
        <button type="button" onClick={onClose} className="rounded-lg p-2 text-[#8e8880] hover:bg-white/8 hover:text-white lg:hidden" aria-label="Close navigation">
          <X className="h-5 w-5" />
        </button>
      </div>

      <div className="mt-7 rounded-xl border border-white/[0.06] bg-white/[0.035] px-3 py-2.5">
        <div className="flex items-center gap-2.5">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-[#315e4f] text-white">
            <Sparkles className="h-3.5 w-3.5" />
          </span>
          <div className="min-w-0 flex-1">
            <p className="truncate text-[0.72rem] font-medium text-[#ded9d1]">The Ember Table</p>
            <p className="mt-0.5 text-[0.58rem] text-[#77726b]">Downtown location</p>
          </div>
          <ChevronDown className="h-3.5 w-3.5 text-[#77726b]" />
        </div>
      </div>

      <nav className="mt-6 flex-1 overflow-y-auto hide-scrollbar">
        <p className="mb-2 px-3 text-[0.57rem] font-semibold uppercase tracking-[0.18em] text-[#5f5b55]">Workspace</p>
        <div className="space-y-1">
          {primaryNavigation.map((item) => <NavLink key={item.href} item={item} onNavigate={onClose} />)}
        </div>

        <p className="mb-2 mt-6 px-3 text-[0.57rem] font-semibold uppercase tracking-[0.18em] text-[#5f5b55]">Insights</p>
        <div className="space-y-1">
          {insightNavigation.map((item) => <NavLink key={item.href} item={item} onNavigate={onClose} />)}
        </div>
      </nav>

      <div className="mt-4 space-y-1 border-t border-white/[0.06] pt-4">
        <NavLink item={{ href: "/dashboard/settings", label: "Settings", icon: Settings }} onNavigate={onClose} />
        <button type="button" className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-[0.8rem] font-medium text-[#aaa39a] hover:bg-white/6 hover:text-white">
          <CircleHelp className="h-[1.1rem] w-[1.1rem] text-[#858078]" strokeWidth={1.8} />
          Help center
        </button>
      </div>

      <div className="mt-3 flex items-center gap-2.5 rounded-xl bg-white/[0.045] p-2.5">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-[#e6b95f] text-xs font-semibold text-[#332510]">{initials}</span>
        <div className="min-w-0 flex-1">
          <p className="truncate text-[0.72rem] font-medium text-[#e9e4dc]">{session.fullName}</p>
          <p className="mt-0.5 truncate text-[0.58rem] text-[#77726b]">{session.roles.join(", ") || "Manager"}</p>
        </div>
        <button
          type="button"
          onClick={() => {
            clearAuthSession();
            router.replace("/");
          }}
          className="rounded-lg p-2 text-[#77726b] hover:bg-white/8 hover:text-white"
          aria-label="Log out"
        >
          <LogOut className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}

export function DashboardShell({ session, children }: DashboardShellProps) {
  const pathname = usePathname();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [currentTime, setCurrentTime] = useState("");
  const pageInfo = routeTitles[pathname] ?? routeTitles["/dashboard"];

  useEffect(() => {
    const update = () => setCurrentTime(new Intl.DateTimeFormat("en-US", { hour: "numeric", minute: "2-digit" }).format(new Date()));
    update();
    const timer = window.setInterval(update, 60_000);
    return () => window.clearInterval(timer);
  }, []);

  return (
    <div className="min-h-screen bg-[#f6f4ef]">
      {sidebarOpen ? (
        <button aria-label="Close navigation" className="fixed inset-0 z-40 bg-black/35 backdrop-blur-[2px] lg:hidden" onClick={() => setSidebarOpen(false)} />
      ) : null}

      <aside className={`fixed inset-y-0 left-0 z-50 w-[15.5rem] transform transition-transform duration-200 lg:translate-x-0 ${sidebarOpen ? "translate-x-0" : "-translate-x-full"}`}>
        <Sidebar session={session} onClose={() => setSidebarOpen(false)} />
      </aside>

      <div className="min-h-screen lg:pl-[15.5rem]">
        <header className="sticky top-0 z-30 border-b border-[#e9e5dd] bg-[#f6f4ef]/90 px-4 py-3 backdrop-blur-xl sm:px-6 lg:px-8">
          <div className="mx-auto flex max-w-[95rem] items-center gap-3">
            <button type="button" onClick={() => setSidebarOpen(true)} className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl border border-[#e4dfd6] bg-white text-[#5c554d] lg:hidden" aria-label="Open navigation">
              <Menu className="h-5 w-5" />
            </button>

            <div className="min-w-0 flex-1">
              <p className="eyebrow hidden sm:block">{pageInfo.eyebrow}</p>
              <h1 className="truncate text-lg font-semibold tracking-[-0.035em] text-[#25211d] sm:mt-0.5 sm:text-xl">{pageInfo.title}</h1>
            </div>

            <label className="hidden h-10 w-full max-w-[15rem] items-center gap-2.5 rounded-xl border border-[#e7e2da] bg-white px-3 text-[#9b948b] xl:flex">
              <Search className="h-4 w-4" />
              <input className="min-w-0 flex-1 border-none bg-transparent text-xs text-[#454039] outline-none placeholder:text-[#a8a198]" placeholder="Search anything..." />
              <kbd className="rounded-md bg-[#f4f1ec] px-1.5 py-0.5 text-[0.58rem] font-medium">⌘K</kbd>
            </label>

            <div className="hidden items-center gap-2 rounded-xl border border-[#e7e2da] bg-white px-3 py-2 text-[0.68rem] font-medium text-[#666057] md:flex">
              <Clock3 className="h-3.5 w-3.5 text-[#e76535]" />
              {currentTime || "—"}
              <span className="h-1.5 w-1.5 rounded-full bg-[#4f9a72]" />
              Open
            </div>

            <button type="button" className="relative flex h-10 w-10 shrink-0 items-center justify-center rounded-xl border border-[#e7e2da] bg-white text-[#5d574f] hover:border-[#d8d1c7]" aria-label="Notifications">
              <Bell className="h-[1.05rem] w-[1.05rem]" />
              <span className="absolute right-2 top-2 h-1.5 w-1.5 rounded-full bg-[#e76535] ring-2 ring-white" />
            </button>

            <Link href="/dashboard/orders?new=1" className="btn-primary h-10 shrink-0 px-3 sm:px-4">
              <Plus className="h-4 w-4" />
              <span className="hidden sm:inline">New order</span>
            </Link>
          </div>
        </header>

        <main className="mx-auto max-w-[95rem] px-4 py-5 sm:px-6 sm:py-6 lg:px-8 lg:py-7">
          <div className="page-enter" key={pathname}>{children}</div>
        </main>
      </div>
    </div>
  );
}
