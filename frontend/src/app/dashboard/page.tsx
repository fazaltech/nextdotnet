"use client";

import { startTransition, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  API_BASE_URL,
  clearAuthSession,
  loadAuthSession,
  type LoginResponse,
} from "@/lib/auth";

export default function DashboardPage() {
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
      <main className="flex min-h-screen items-center justify-center bg-[linear-gradient(135deg,#eff6ff_0%,#f5f3ff_48%,#fdf2f8_100%)] px-6 py-10">
        <div className="rounded-[1.75rem] bg-white/90 px-8 py-6 text-sm font-medium text-slate-500 shadow-[0_24px_60px_rgba(76,55,148,0.18)]">
          Loading dashboard...
        </div>
      </main>
    );
  }

  const expiresAt = new Date(session.expiresAtUtc).toLocaleString();
  const tokenPreview = `${session.accessToken.slice(0, 24)}...${session.accessToken.slice(-18)}`;

  return (
    <main className="min-h-screen bg-[linear-gradient(135deg,#eff6ff_0%,#f5f3ff_48%,#fdf2f8_100%)] px-4 py-6 sm:px-6 lg:px-10">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        <section className="overflow-hidden rounded-[2rem] bg-[linear-gradient(135deg,#312e81_0%,#6d28d9_44%,#db2777_100%)] text-white shadow-[0_30px_80px_rgba(76,55,148,0.3)]">
          <div className="flex flex-col gap-8 px-6 py-8 sm:px-8 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-sm font-medium uppercase tracking-[0.3em] text-white/70">
                Dashboard
              </p>
              <h1 className="mt-3 text-3xl font-semibold tracking-[-0.04em] sm:text-4xl">
                Welcome back, {session.fullName}
              </h1>
              <p className="mt-3 max-w-2xl text-sm text-white/80 sm:text-base">
                You are signed in as {session.userName} and can now call secured
                backend APIs using the stored bearer token.
              </p>
            </div>

            <button
              type="button"
              onClick={() => {
                clearAuthSession();
                startTransition(() => {
                  router.replace("/");
                });
              }}
              className="inline-flex items-center justify-center rounded-full border border-white/30 bg-white/10 px-5 py-3 text-sm font-semibold tracking-[0.14em] text-white transition-colors hover:bg-white/20"
            >
              LOG OUT
            </button>
          </div>
        </section>

        <section className="grid gap-4 md:grid-cols-3">
          <article className="rounded-[1.5rem] bg-white p-5 shadow-[0_18px_50px_rgba(76,55,148,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              API Base
            </p>
            <p className="mt-3 break-all text-sm font-medium text-slate-700">
              {API_BASE_URL}
            </p>
          </article>

          <article className="rounded-[1.5rem] bg-white p-5 shadow-[0_18px_50px_rgba(76,55,148,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Role Access
            </p>
            <p className="mt-3 text-2xl font-semibold tracking-[-0.04em] text-slate-900">
              {session.roles.join(", ")}
            </p>
          </article>

          <article className="rounded-[1.5rem] bg-white p-5 shadow-[0_18px_50px_rgba(76,55,148,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Session Expiry
            </p>
            <p className="mt-3 text-sm font-medium text-slate-700">{expiresAt}</p>
          </article>
        </section>

        <section className="grid gap-6 lg:grid-cols-[1.3fr_0.9fr]">
          <article className="rounded-[1.8rem] bg-white p-6 shadow-[0_20px_60px_rgba(76,55,148,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Account Summary
            </p>
            <div className="mt-5 grid gap-4 sm:grid-cols-2">
              <div className="rounded-[1.25rem] bg-slate-50 p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-slate-400">
                  User Name
                </p>
                <p className="mt-2 text-lg font-semibold text-slate-900">
                  {session.userName}
                </p>
              </div>
              <div className="rounded-[1.25rem] bg-slate-50 p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-slate-400">
                  Email
                </p>
                <p className="mt-2 text-lg font-semibold text-slate-900">
                  {session.email}
                </p>
              </div>
              <div className="rounded-[1.25rem] bg-slate-50 p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-slate-400">
                  User Id
                </p>
                <p className="mt-2 text-lg font-semibold text-slate-900">
                  #{session.userId}
                </p>
              </div>
              <div className="rounded-[1.25rem] bg-slate-50 p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-slate-400">
                  Auth Route
                </p>
                <p className="mt-2 text-sm font-semibold text-slate-900">
                  /auth/login
                </p>
              </div>
            </div>
          </article>

          <article className="rounded-[1.8rem] bg-slate-950 p-6 text-white shadow-[0_20px_60px_rgba(15,23,42,0.34)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Bearer Token
            </p>
            <p className="mt-4 rounded-[1.25rem] bg-white/5 p-4 text-xs leading-6 text-slate-300">
              {tokenPreview}
            </p>
            <p className="mt-4 text-sm text-slate-400">
              Stored in local storage for the current browser session.
            </p>
          </article>
        </section>
      </div>
    </main>
  );
}
