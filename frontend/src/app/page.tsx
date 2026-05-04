export default function Home() {
  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-8 sm:px-6">
      <div className="pointer-events-none absolute inset-0 bg-[linear-gradient(135deg,#5ae4ef_0%,#7f68ff_46%,#e538dd_100%)]" />
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute left-[-8%] top-[6%] h-60 w-60 rounded-[3rem] bg-white/10 [clip-path:polygon(0_0,100%_12%,60%_100%,8%_66%)]" />
        <div className="absolute bottom-[-6%] left-[8%] h-72 w-80 bg-white/8 [clip-path:polygon(0_22%,100%_0,74%_100%)]" />
        <div className="absolute right-[-2%] top-[18%] h-72 w-72 bg-black/8 [clip-path:polygon(24%_0,100%_26%,82%_100%,0_42%)]" />
        <div className="absolute bottom-[-10%] right-[-8%] h-96 w-80 bg-black/12 [clip-path:polygon(0_0,100%_18%,100%_100%,34%_70%)]" />
      </div>

      <section className="relative z-10 w-full max-w-5xl rounded-[2rem] bg-white/14 p-3 shadow-[0_24px_64px_rgba(28,18,64,0.22)] backdrop-blur-[2px] sm:p-6">
       <div className="relative flex min-h-[720px] items-center justify-center px-4 py-10 sm:px-10">
            <div className="w-full max-w-[22rem] rounded-[1.75rem] bg-white px-7 py-10 text-center shadow-[0_28px_80px_rgba(58,34,118,0.14)] sm:px-9">
              <h1 className="text-[2.1rem] font-semibold tracking-[-0.03em] text-slate-900">
                Login
              </h1>

              <form className="mt-10 space-y-6 text-left">
                <div>
                  <label
                    htmlFor="username"
                    className="mb-2 block text-[0.8rem] font-medium text-slate-500"
                  >
                    Username
                  </label>
                  <div className="flex items-center gap-3 border-b border-slate-200 pb-3 text-slate-400 transition-colors focus-within:border-fuchsia-500">
                    <svg
                      aria-hidden="true"
                      viewBox="0 0 24 24"
                      className="h-4 w-4 shrink-0"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                    >
                      <path d="M20 21a8 8 0 1 0-16 0" />
                      <circle cx="12" cy="7" r="4" />
                    </svg>
                    <input
                      id="username"
                      name="username"
                      type="text"
                      placeholder="Type your username"
                      className="w-full border-none bg-transparent text-sm text-slate-700 outline-none placeholder:text-slate-400"
                    />
                  </div>
                </div>

                <div>
                  <label
                    htmlFor="password"
                    className="mb-2 block text-[0.8rem] font-medium text-slate-500"
                  >
                    Password
                  </label>
                  <div className="flex items-center gap-3 border-b border-slate-200 pb-3 text-slate-400 transition-colors focus-within:border-fuchsia-500">
                    <svg
                      aria-hidden="true"
                      viewBox="0 0 24 24"
                      className="h-4 w-4 shrink-0"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                    >
                      <rect x="5" y="11" width="14" height="10" rx="2" />
                      <path d="M8 11V8a4 4 0 1 1 8 0v3" />
                    </svg>
                    <input
                      id="password"
                      name="password"
                      type="password"
                      placeholder="Type your password"
                      className="w-full border-none bg-transparent text-sm text-slate-700 outline-none placeholder:text-slate-400"
                    />
                  </div>
                </div>

                <div className="text-right">
                  <a
                    href="#"
                    className="text-[0.78rem] font-medium text-slate-500 transition-colors hover:text-fuchsia-600"
                  >
                    Forgot password?
                  </a>
                </div>

                <button
                  type="submit"
                  className="block w-full rounded-full bg-[linear-gradient(90deg,#58e3f0_0%,#7d67ff_45%,#e538dd_100%)] px-5 py-3 text-sm font-semibold tracking-[0.18em] text-white shadow-[0_12px_28px_rgba(140,76,255,0.35)] transition-transform hover:-translate-y-0.5"
                >
                  LOGIN
                </button>
              </form>

              <div className="mt-10 text-center">
                <p className="text-xs text-slate-400">Or Sign Up Using</p>
                <div className="mt-5 flex items-center justify-center gap-4">
                  <a
                    href="#"
                    aria-label="Sign in with Facebook"
                    className="flex h-11 w-11 items-center justify-center rounded-full bg-[#3b5998] text-white shadow-lg shadow-[#3b5998]/20 transition-transform hover:-translate-y-0.5"
                  >
                    <span className="text-lg font-semibold">f</span>
                  </a>
                  <a
                    href="#"
                    aria-label="Sign in with Twitter"
                    className="flex h-11 w-11 items-center justify-center rounded-full bg-[#1da1f2] text-white shadow-lg shadow-[#1da1f2]/20 transition-transform hover:-translate-y-0.5"
                  >
                    <span className="text-lg font-semibold">t</span>
                  </a>
                  <a
                    href="#"
                    aria-label="Sign in with Google"
                    className="flex h-11 w-11 items-center justify-center rounded-full bg-[#ea4335] text-white shadow-lg shadow-[#ea4335]/20 transition-transform hover:-translate-y-0.5"
                  >
                    <span className="text-lg font-semibold">G</span>
                  </a>
                </div>
              </div>

              <div className="mt-20 text-center">
                <p className="text-xs text-slate-400">Or Sign Up Using</p>
                <a
                  href="#"
                  className="mt-4 inline-block text-sm font-semibold tracking-[0.14em] text-slate-700 transition-colors hover:text-fuchsia-600"
                >
                  SIGN UP
                </a>
              </div>
            </div>
          </div>
      </section>
    </main>
  );
}
