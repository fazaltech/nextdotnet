"use client";

import { startTransition, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { fetchWithAuth } from "@/components/dashboard/dashboard-shell";
import { buildApiUrl } from "@/lib/config";

type CustomerDto = {
  id: number;
  fullName: string;
  phone: string | null;
  email: string | null;
  isActive: boolean;
};

export default function CustomersPage() {
  const router = useRouter();
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadCustomers() {
      setIsLoading(true);
      setErrorMessage("");

      try {
        const rows = await fetchWithAuth<CustomerDto[]>(buildApiUrl("customers"));
        if (!cancelled) {
          setCustomers(rows);
        }
      } catch (error) {
        if (cancelled) {
          return;
        }

        const message =
          error instanceof Error ? error.message : "Unable to load customers.";

        if (message === "AUTH_REQUIRED" || message === "AUTH_EXPIRED") {
          startTransition(() => {
            router.replace("/");
          });
          return;
        }

        setErrorMessage("Unable to load customers from the backend API.");
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    loadCustomers();

    return () => {
      cancelled = true;
    };
  }, [router]);

  return (
    <div className="space-y-6">
      <section className="rounded-[1.8rem] bg-white p-6 shadow-[0_18px_60px_rgba(56,72,107,0.12)]">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Customer Directory
            </p>
            <h2 className="mt-2 text-3xl font-semibold tracking-[-0.04em] text-slate-900">
              Customer List
            </h2>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
              Live customer data from the secured backend endpoint. This page
              uses the stored bearer token to call `/api/customers`.
            </p>
          </div>

          <div className="rounded-[1.5rem] bg-[#12365e] px-5 py-4 text-white shadow-[0_18px_36px_rgba(18,54,94,0.24)]">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-white/70">
              Records
            </p>
            <p className="mt-2 text-3xl font-semibold tracking-[-0.04em]">
              {customers.length}
            </p>
          </div>
        </div>
      </section>

      <section className="rounded-[1.8rem] bg-white p-4 shadow-[0_18px_60px_rgba(56,72,107,0.12)] sm:p-6">
        {isLoading ? (
          <div className="grid gap-3">
            {Array.from({ length: 5 }).map((_, index) => (
              <div
                key={index}
                className="h-16 animate-pulse rounded-[1.2rem] bg-slate-100"
              />
            ))}
          </div>
        ) : errorMessage ? (
          <div className="rounded-[1.25rem] bg-rose-50 px-5 py-4 text-sm font-medium text-rose-600">
            {errorMessage}
          </div>
        ) : customers.length === 0 ? (
          <div className="rounded-[1.25rem] border border-dashed border-slate-200 px-5 py-10 text-center text-sm text-slate-500">
            No customers found in the database yet.
          </div>
        ) : (
          <div className="overflow-hidden rounded-[1.4rem] border border-slate-200">
            <div className="grid grid-cols-[1.2fr_1fr_1fr_auto] gap-3 bg-slate-50 px-5 py-4 text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
              <span>Name</span>
              <span>Phone</span>
              <span>Email</span>
              <span>Status</span>
            </div>

            <div className="divide-y divide-slate-100">
              {customers.map((customer) => (
                <div
                  key={customer.id}
                  className="grid grid-cols-1 gap-3 px-5 py-4 text-sm text-slate-600 sm:grid-cols-[1.2fr_1fr_1fr_auto] sm:items-center"
                >
                  <div>
                    <p className="font-semibold text-slate-900">
                      {customer.fullName}
                    </p>
                    <p className="mt-1 text-xs uppercase tracking-[0.16em] text-slate-400">
                      Customer #{customer.id}
                    </p>
                  </div>
                  <span>{customer.phone || "Not provided"}</span>
                  <span className="break-all">{customer.email || "Not provided"}</span>
                  <span
                    className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-[0.12em] ${
                      customer.isActive
                        ? "bg-emerald-100 text-emerald-700"
                        : "bg-slate-200 text-slate-600"
                    }`}
                  >
                    {customer.isActive ? "Active" : "Inactive"}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </section>
    </div>
  );
}
