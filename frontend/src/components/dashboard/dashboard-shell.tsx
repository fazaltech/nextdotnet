"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  createAuthHeaders,
  clearAuthSession,
  loadAuthSession,
  type LoginResponse,
} from "@/lib/auth";

type DashboardShellProps = {
  session: LoginResponse;
  children: React.ReactNode;
};

type NavItem = {
  href: string;
  label: string;
  icon: React.ReactNode;
};

const navItems: NavItem[] = [
  {
    href: "/dashboard",
    label: "Home",
    icon: (
      <path d="M4 11.5 12 5l8 6.5V20a1 1 0 0 1-1 1h-4.5v-5h-5v5H5a1 1 0 0 1-1-1z" />
    ),
  },
  {
    href: "/dashboard/customers",
    label: "Customers",
    icon: (
      <>
        <circle cx="9" cy="9" r="3" />
        <circle cx="17" cy="10.5" r="2.5" />
        <path d="M3.5 20a5.5 5.5 0 0 1 11 0" />
        <path d="M14 20a4 4 0 0 1 7 0" />
      </>
    ),
  },
  {
    href: "/dashboard/messages",
    label: "Messages",
    icon: (
      <>
        <path d="M4 6.5A2.5 2.5 0 0 1 6.5 4h11A2.5 2.5 0 0 1 20 6.5v7A2.5 2.5 0 0 1 17.5 16H9l-5 4v-3.9A2.5 2.5 0 0 1 4 13.5z" />
        <path d="m7 8 5 4 5-4" />
      </>
    ),
  },
  {
    href: "/dashboard/analytics",
    label: "Analytics",
    icon: (
      <>
        <path d="M5 18V9" />
        <path d="M12 18V5" />
        <path d="M19 18v-7" />
      </>
    ),
  },
];

function DashboardIcon({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-5 w-5"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.7"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {children}
    </svg>
  );
}

export function DashboardShell({
  session,
  children,
}: DashboardShellProps) {
  const pathname = usePathname();
  const router = useRouter();

  const initials = session.fullName
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");

  return (
    <div className="min-h-screen bg-[#dbe1eb] px-3 py-4 sm:px-5 lg:px-8 lg:py-7">
      <div className="mx-auto flex min-h-[calc(100vh-2rem)] max-w-7xl overflow-hidden rounded-[2rem] bg-[#eef2f8] shadow-[0_34px_80px_rgba(15,23,42,0.18)] ring-1 ring-white/50">
        <aside className="flex w-full max-w-[18rem] flex-col bg-[linear-gradient(180deg,#12365e_0%,#102b49_100%)] px-6 py-7 text-white lg:w-[18rem]">
          <div className="mx-auto flex h-24 w-24 items-center justify-center rounded-full border border-white/10 bg-[radial-gradient(circle_at_30%_30%,#ffffff_0%,#dde7f5_30%,#a9bfd8_31%,#183b60_100%)] text-3xl font-semibold text-slate-900 shadow-[0_16px_28px_rgba(0,0,0,0.28)]">
            {initials}
          </div>

          <div className="mt-6 text-center">
            <h2 className="text-[1.7rem] font-semibold uppercase tracking-[0.08em]">
              {session.fullName}
            </h2>
            <p className="mt-2 break-all text-sm text-slate-300">{session.email}</p>
          </div>

          <nav className="mt-10 space-y-2">
            {navItems.map((item) => {
              const isActive =
                pathname === item.href ||
                (item.href !== "/dashboard" && pathname.startsWith(`${item.href}/`));

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition-all ${
                    isActive
                      ? "bg-white text-[#12365e] shadow-[0_16px_28px_rgba(0,0,0,0.16)]"
                      : "text-slate-200 hover:bg-white/8 hover:text-white"
                  }`}
                >
                  <DashboardIcon>{item.icon}</DashboardIcon>
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </nav>

          <div className="mt-auto rounded-[1.5rem] border border-white/10 bg-white/6 p-4 text-sm text-slate-200">
            <p className="font-semibold uppercase tracking-[0.16em] text-slate-300">
              Session
            </p>
            <p className="mt-3 text-xs leading-6 text-slate-300/90">
              Authenticated requests use the stored bearer token.
            </p>
            <button
              type="button"
              onClick={() => {
                clearAuthSession();
                router.replace("/");
              }}
              className="mt-4 inline-flex items-center justify-center rounded-full bg-white px-4 py-2 text-xs font-semibold tracking-[0.18em] text-[#12365e] transition-transform hover:-translate-y-0.5"
            >
              LOG OUT
            </button>
          </div>
        </aside>

        <div className="flex min-w-0 flex-1 flex-col bg-[#f7f9fc]">
          <header className="flex flex-col gap-4 border-b border-slate-200/80 px-5 py-5 sm:px-7 lg:flex-row lg:items-center lg:justify-between lg:px-8">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-slate-400">
                Restaurant Suite
              </p>
              <h1 className="mt-2 text-2xl font-semibold tracking-[-0.04em] text-slate-900">
                Dashboard User
              </h1>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-500 shadow-sm">
                {session.roles.join(", ")}
              </div>
              <div className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-500 shadow-sm">
                {loadAuthSession() ? "Bearer Active" : "Signed Out"}
              </div>
              <button
                type="button"
                className="inline-flex h-11 w-11 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-600 shadow-sm transition-colors hover:text-slate-900"
                aria-label="Dashboard menu"
              >
                <DashboardIcon>
                  <path d="M5 7h14" />
                  <path d="M5 12h14" />
                  <path d="M9 17h10" />
                </DashboardIcon>
              </button>
            </div>
          </header>

          <main className="min-h-0 flex-1 overflow-auto px-5 py-5 sm:px-7 sm:py-6 lg:px-8">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}

export async function fetchWithAuth<T>(path: string): Promise<T> {
  const session = loadAuthSession();
  if (!session) {
    throw new Error("AUTH_REQUIRED");
  }

  const response = await fetch(path, {
    headers: createAuthHeaders(session.accessToken),
  });

  if (response.status === 401) {
    clearAuthSession();
    throw new Error("AUTH_EXPIRED");
  }

  if (!response.ok) {
    throw new Error(`REQUEST_FAILED:${response.status}`);
  }

  return (await response.json()) as T;
}
