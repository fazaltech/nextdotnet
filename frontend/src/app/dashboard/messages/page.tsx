export default function MessagesPage() {
  return (
    <section className="rounded-[1.8rem] bg-white p-8 shadow-[0_18px_48px_rgba(56,72,107,0.12)]">
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">
        Messages
      </p>
      <h2 className="mt-3 text-3xl font-semibold tracking-[-0.04em] text-slate-900">
        Message Center
      </h2>
      <p className="mt-4 max-w-2xl text-sm leading-6 text-slate-500">
        This route is available from the sidebar and can be extended for inbox,
        staff communication, or system notifications without changing the
        dashboard shell.
      </p>
    </section>
  );
}
