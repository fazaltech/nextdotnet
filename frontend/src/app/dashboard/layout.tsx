"use client";

import { startTransition, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { DashboardShell } from "@/components/dashboard/dashboard-shell";
import { loadAuthSession, type LoginResponse } from "@/lib/auth";

export default function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const router = useRouter();
  const [session, setSession] = useState<LoginResponse | null>(null);

  useEffect(() => {
    const currentSession = loadAuthSession();
    if (!currentSession) {
      startTransition(() => {
        router.replace("/");
      });
      return;
    }

    setSession(currentSession);
  }, [router]);

  if (!session) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-[#dbe1eb] px-6 py-10">
        <div className="rounded-[1.75rem] bg-white px-8 py-6 text-sm font-medium text-slate-500 shadow-[0_24px_60px_rgba(76,55,148,0.18)]">
          Loading dashboard...
        </div>
      </main>
    );
  }

  return <DashboardShell session={session}>{children}</DashboardShell>;
}
