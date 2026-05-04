export default function DashboardPage() {
  return (
    <div className="grid gap-6">
      <section className="grid gap-4 xl:grid-cols-[1.65fr_0.75fr]">
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <article className="rounded-[1.6rem] bg-[#11365e] p-5 text-white shadow-[0_20px_48px_rgba(17,54,94,0.28)]">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-white/70">
                Earning
              </span>
              <span className="rounded-full bg-white/16 px-2.5 py-1 text-[11px] font-semibold">
                $
              </span>
            </div>
            <p className="mt-7 text-4xl font-semibold tracking-[-0.04em]">$ 628</p>
          </article>

          <article className="rounded-[1.6rem] bg-white p-5 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                Share
              </span>
              <span className="text-orange-400">
                <svg
                  aria-hidden="true"
                  viewBox="0 0 24 24"
                  className="h-5 w-5"
                  fill="currentColor"
                >
                  <path d="M18 8a3 3 0 1 0-2.82-4H15a3 3 0 0 0 .18 1.02l-6.1 3.05a3 3 0 0 0-2.26-1.03 3 3 0 1 0 2.38 4.83l5.9 3.18A3 3 0 0 0 15 16a3 3 0 1 0 .35-1.4l-5.82-3.14a3.2 3.2 0 0 0 .06-.58c0-.2-.02-.4-.06-.58l6.1-3.05A3 3 0 0 0 18 8Z" />
                </svg>
              </span>
            </div>
            <p className="mt-7 text-4xl font-semibold tracking-[-0.04em] text-slate-900">
              2434
            </p>
          </article>

          <article className="rounded-[1.6rem] bg-white p-5 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                Likes
              </span>
              <span className="text-orange-400">👍</span>
            </div>
            <p className="mt-7 text-4xl font-semibold tracking-[-0.04em] text-slate-900">
              1259
            </p>
          </article>

          <article className="rounded-[1.6rem] bg-white p-5 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                Rating
              </span>
              <span className="text-orange-400">★</span>
            </div>
            <p className="mt-7 text-4xl font-semibold tracking-[-0.04em] text-slate-900">
              8,5
            </p>
          </article>
        </div>

        <div className="rounded-[1.8rem] bg-white p-6 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
          <div className="mx-auto flex h-44 w-44 items-center justify-center rounded-full bg-[conic-gradient(#ffad1f_0_162deg,#173a63_162deg_360deg)]">
            <div className="flex h-28 w-28 items-center justify-center rounded-full bg-white text-3xl font-semibold text-slate-900">
              45%
            </div>
          </div>
          <div className="mt-6 space-y-3 text-sm text-slate-500">
            <p>Lorem ipsum</p>
            <p>Lorem ipsum</p>
            <p>Lorem ipsum</p>
            <p>Lorem ipsum</p>
          </div>
          <button
            type="button"
            className="mt-6 rounded-full bg-[#ffad1f] px-5 py-2.5 text-sm font-semibold text-white shadow-[0_12px_24px_rgba(255,173,31,0.32)]"
          >
            Check Now
          </button>
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-[1.65fr_0.75fr]">
        <div className="space-y-6">
          <article className="rounded-[1.8rem] bg-white p-5 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
              <p className="text-sm font-semibold text-slate-500">Result</p>
              <div className="flex items-center gap-3 text-xs">
                <span className="rounded-full bg-[#163860] px-3 py-1 font-semibold text-white">
                  2025
                </span>
                <button
                  type="button"
                  className="rounded-full bg-[#ffad1f] px-4 py-1.5 font-semibold text-white"
                >
                  Check Now
                </button>
              </div>
            </div>

            <div className="mt-6 grid h-56 grid-cols-9 items-end gap-3">
              {[
                [44, 28],
                [58, 35],
                [40, 18],
                [33, 26],
                [41, 31],
                [52, 29],
                [37, 18],
                [30, 24],
                [34, 45],
              ].map(([blue, orange], index) => (
                <div key={index} className="flex h-full items-end justify-center gap-2">
                  <div
                    className="w-4 rounded-t-full bg-[#15375f]"
                    style={{ height: `${blue}%` }}
                  />
                  <div
                    className="w-4 rounded-t-full bg-[#ffad1f]"
                    style={{ height: `${orange}%` }}
                  />
                </div>
              ))}
            </div>

            <div className="mt-4 grid grid-cols-9 text-center text-[10px] font-semibold uppercase tracking-[0.16em] text-slate-400">
              {["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep"].map(
                (month) => (
                  <span key={month}>{month}</span>
                ),
              )}
            </div>
          </article>

          <article className="rounded-[1.8rem] bg-white p-5 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-4 text-xs font-medium text-slate-400">
                  <span className="inline-flex items-center gap-2">
                    <span className="h-2.5 w-2.5 rounded-full bg-[#ffad1f]" />
                    Lorem Ipsum
                  </span>
                  <span className="inline-flex items-center gap-2">
                    <span className="h-2.5 w-2.5 rounded-full bg-[#15375f]" />
                    Dolor Amet
                  </span>
                </div>

                <div className="mt-8 h-40 rounded-[1.5rem] bg-[linear-gradient(180deg,rgba(255,173,31,0.18)_0%,rgba(255,173,31,0.03)_100%)] p-4">
                  <div className="h-full w-full rounded-[1.2rem] bg-[radial-gradient(circle_at_20%_65%,rgba(255,173,31,0.95)_0_9%,transparent_10%),radial-gradient(circle_at_34%_40%,rgba(22,56,96,0.7)_0_12%,transparent_13%),radial-gradient(circle_at_52%_72%,rgba(255,173,31,0.95)_0_10%,transparent_11%),radial-gradient(circle_at_72%_50%,rgba(22,56,96,0.7)_0_12%,transparent_13%),radial-gradient(circle_at_88%_62%,rgba(255,173,31,0.95)_0_9%,transparent_10%),linear-gradient(180deg,rgba(22,56,96,0.18),rgba(22,56,96,0))]" />
                </div>
              </div>

              <div className="w-full max-w-xs rounded-[1.5rem] bg-slate-50 p-4">
                <div className="grid grid-cols-7 gap-2 text-center text-[10px] font-semibold uppercase tracking-[0.14em] text-slate-400">
                  {["S", "M", "T", "W", "T", "F", "S"].map((day, index) => (
                    <span key={`${day}-${index}`}>{day}</span>
                  ))}
                </div>
                <div className="mt-3 grid grid-cols-7 gap-2 text-center text-xs font-medium text-slate-500">
                  {Array.from({ length: 28 }).map((_, index) => {
                    const day = index + 1;
                    const isHighlight = day === 10 || day === 18 || day === 23;
                    const isDark = day === 12 || day === 19;

                    return (
                      <span
                        key={day}
                        className={`rounded-lg px-2 py-2 ${
                          isDark
                            ? "bg-[#15375f] text-white"
                            : isHighlight
                              ? "bg-[#ffad1f] text-white"
                              : "bg-white"
                        }`}
                      >
                        {day}
                      </span>
                    );
                  })}
                </div>
              </div>
            </div>
          </article>
        </div>

        <aside className="space-y-6">
          <article className="rounded-[1.8rem] bg-white p-6 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">
              Overview
            </p>
            <div className="mt-5 space-y-4">
              <div className="rounded-[1.2rem] bg-[#edf3fb] p-4">
                <p className="text-xs uppercase tracking-[0.16em] text-slate-400">
                  Tables In Use
                </p>
                <p className="mt-2 text-3xl font-semibold tracking-[-0.04em] text-slate-900">
                  12
                </p>
              </div>
              <div className="rounded-[1.2rem] bg-[#fff5e4] p-4">
                <p className="text-xs uppercase tracking-[0.16em] text-slate-400">
                  New Customers
                </p>
                <p className="mt-2 text-3xl font-semibold tracking-[-0.04em] text-slate-900">
                  38
                </p>
              </div>
              <div className="rounded-[1.2rem] bg-[#f4f1ff] p-4">
                <p className="text-xs uppercase tracking-[0.16em] text-slate-400">
                  Pending Orders
                </p>
                <p className="mt-2 text-3xl font-semibold tracking-[-0.04em] text-slate-900">
                  9
                </p>
              </div>
            </div>
          </article>

          <article className="rounded-[1.8rem] bg-white p-6 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">
              Quick Notes
            </p>
            <div className="mt-5 space-y-3 text-sm text-slate-500">
              <p>Lunch service spike expected at 1:00 PM.</p>
              <p>Kitchen inventory review scheduled for this evening.</p>
              <p>Check customer feedback trends before closing.</p>
            </div>
            <button
              type="button"
              className="mt-6 rounded-full bg-[#ffad1f] px-5 py-2.5 text-sm font-semibold text-white shadow-[0_12px_24px_rgba(255,173,31,0.32)]"
            >
              Check Now
            </button>
          </article>
        </aside>
      </section>
    </div>
  );
}
