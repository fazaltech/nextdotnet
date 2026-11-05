"use client";

import { type FormEvent, startTransition, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowRight,
  BarChart3,
  ChefHat,
  Eye,
  EyeOff,
  LockKeyhole,
  ShieldCheck,
  Sparkles,
  UserRound,
  UtensilsCrossed,
} from "lucide-react";
import {
  LOGIN_ENDPOINT,
  loadAuthSession,
  saveAuthSession,
  type LoginResponse,
} from "@/lib/auth";

export default function Home() {
  const router = useRouter();
  const [userNameOrEmail, setUserNameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (loadAuthSession()) {
      startTransition(() => router.replace("/dashboard"));
    }
  }, [router]);

  function enterDashboard(session: LoginResponse) {
    saveAuthSession(session);
    startTransition(() => router.push("/dashboard"));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      const response = await fetch(LOGIN_ENDPOINT, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userNameOrEmail, password }),
      });

      if (!response.ok) {
        setErrorMessage("Those details did not match an active account.");
        return;
      }

      enterDashboard((await response.json()) as LoginResponse);
    } catch {
      setErrorMessage("The sign-in service is currently unavailable. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="min-h-screen bg-[#1d1c19] p-3 sm:p-5 lg:p-6">
      <div className="mx-auto grid min-h-[calc(100vh-1.5rem)] max-w-[92rem] overflow-hidden rounded-[1.75rem] bg-[#f7f4ee] shadow-2xl shadow-black/20 sm:min-h-[calc(100vh-2.5rem)] lg:grid-cols-[1.08fr_0.92fr]">
        <section className="relative hidden overflow-hidden bg-[#24221e] p-10 text-white lg:flex lg:flex-col xl:p-14">
          <div className="soft-grid absolute inset-0 opacity-[0.14]" />
          <div className="absolute -right-24 -top-24 h-80 w-80 rounded-full bg-[#e76535]/20 blur-3xl" />
          <div className="absolute -bottom-36 -left-24 h-96 w-96 rounded-full bg-[#d9ae58]/10 blur-3xl" />

          <div className="relative z-10 flex items-center gap-3">
            <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-[#e76535] shadow-[0_10px_30px_rgba(231,101,53,0.3)]">
              <ChefHat className="h-6 w-6" />
            </span>
            <div>
              <p className="text-xl font-semibold tracking-[-0.04em]">Savorly</p>
              <p className="text-[0.62rem] font-medium uppercase tracking-[0.24em] text-white/35">Restaurant OS</p>
            </div>
          </div>

          <div className="relative z-10 my-auto max-w-xl py-12">
            <p className="mb-5 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/[0.04] px-3 py-1.5 text-[0.68rem] font-medium text-white/60">
              <Sparkles className="h-3.5 w-3.5 text-[#e7b95d]" />
              One calm place to run every service
            </p>
            <h1 className="max-w-lg text-[3.5rem] font-semibold leading-[1.07] tracking-[-0.055em] xl:text-[4.35rem]">
              Great hospitality starts behind the scenes.
            </h1>
            <p className="mt-6 max-w-lg text-sm leading-7 text-white/48">
              Keep orders moving, tables turning, inventory stocked, and your team in sync—from first prep to final check.
            </p>

            <div className="mt-10 grid max-w-lg grid-cols-3 gap-3">
              {[
                { icon: UtensilsCrossed, value: "Service", label: "Tables and orders" },
                { icon: BarChart3, value: "Reports", label: "Sales and costs" },
                { icon: ShieldCheck, value: "Access", label: "Role permissions" },
              ].map(({ icon: Icon, value, label }) => (
                <div key={label} className="rounded-2xl border border-white/[0.07] bg-white/[0.035] p-4 backdrop-blur-sm">
                  <Icon className="h-4 w-4 text-[#e76535]" />
                  <p className="mt-5 text-xl font-semibold tracking-[-0.04em]">{value}</p>
                  <p className="mt-1 text-[0.62rem] text-white/35">{label}</p>
                </div>
              ))}
            </div>
          </div>

        </section>

        <section className="flex items-center justify-center px-5 py-10 sm:px-10 lg:px-14 xl:px-20">
          <div className="w-full max-w-[27rem]">
            <div className="mb-10 flex items-center gap-3 lg:hidden">
              <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#e76535] text-white"><ChefHat className="h-5 w-5" /></span>
              <span className="text-lg font-semibold tracking-[-0.04em]">Savorly</span>
            </div>

            <p className="eyebrow">Welcome back</p>
            <h2 className="mt-3 text-3xl font-semibold tracking-[-0.05em] text-[#25211d] sm:text-[2.4rem]">Sign in to your restaurant</h2>
            <p className="mt-3 text-sm leading-6 text-[#837c72]">Use your restaurant account to continue.</p>

            <form className="mt-9 space-y-5" onSubmit={handleSubmit}>
              <div>
                <label htmlFor="username" className="mb-2 block text-[0.72rem] font-semibold text-[#4b463f]">Email or username</label>
                <div className="flex h-12 items-center gap-3 rounded-xl border border-[#dfdad1] bg-white px-3.5 transition-colors focus-within:border-[#e76535] focus-within:ring-4 focus-within:ring-[#e76535]/8">
                  <UserRound className="h-4 w-4 text-[#aaa39a]" />
                  <input id="username" type="text" value={userNameOrEmail} onChange={(event) => setUserNameOrEmail(event.target.value)} autoComplete="username" required className="min-w-0 flex-1 border-none bg-transparent text-sm text-[#332e29] outline-none placeholder:text-[#b3aca3]" placeholder="manager@restaurant.com" />
                </div>
              </div>

              <div>
                <div className="mb-2 flex items-center justify-between">
                  <label htmlFor="password" className="text-[0.72rem] font-semibold text-[#4b463f]">Password</label>
                  <span className="text-xs text-stone-500">Ask your owner to reset a forgotten password.</span>
                </div>
                <div className="flex h-12 items-center gap-3 rounded-xl border border-[#dfdad1] bg-white px-3.5 transition-colors focus-within:border-[#e76535] focus-within:ring-4 focus-within:ring-[#e76535]/8">
                  <LockKeyhole className="h-4 w-4 text-[#aaa39a]" />
                  <input id="password" type={showPassword ? "text" : "password"} value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="current-password" required className="min-w-0 flex-1 border-none bg-transparent text-sm text-[#332e29] outline-none placeholder:text-[#b3aca3]" placeholder="Enter your password" />
                  <button type="button" onClick={() => setShowPassword((current) => !current)} className="rounded-lg p-1 text-[#a39c93] hover:text-[#625c54]" aria-label={showPassword ? "Hide password" : "Show password"}>
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
              </div>

              {errorMessage ? <p className="rounded-xl border border-[#f1c9bb] bg-[#fff2ed] px-4 py-3 text-xs leading-5 text-[#ad4522]">{errorMessage}</p> : null}

              <button type="submit" disabled={isSubmitting} className="btn-primary h-12 w-full disabled:cursor-not-allowed disabled:opacity-60">
                {isSubmitting ? "Signing in..." : "Sign in"}
                {!isSubmitting ? <ArrowRight className="h-4 w-4" /> : null}
              </button>
            </form>

            <p className="mt-7 text-center text-[0.66rem] leading-5 text-[#9b948b]">
              Sign in using the account provided by your restaurant owner.
            </p>
          </div>
        </section>
      </div>
    </main>
  );
}
