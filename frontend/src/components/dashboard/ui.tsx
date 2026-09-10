import { ChevronRight } from "lucide-react";
import { getOrderStatus } from "@/lib/restaurant-data";

export function SectionTitle({
  eyebrow,
  title,
  action,
}: {
  eyebrow?: string;
  title: string;
  action?: React.ReactNode;
}) {
  return (
    <div className="flex items-end justify-between gap-4">
      <div>
        {eyebrow ? <p className="eyebrow">{eyebrow}</p> : null}
        <h2 className={`${eyebrow ? "mt-1.5" : ""} text-[1.05rem] font-semibold tracking-[-0.035em] text-[#2b2722]`}>{title}</h2>
      </div>
      {action}
    </div>
  );
}

export function TextLink({ children }: { children: React.ReactNode }) {
  return (
    <span className="inline-flex items-center gap-1 text-[0.68rem] font-semibold text-[#e76535]">
      {children}<ChevronRight className="h-3.5 w-3.5" />
    </span>
  );
}

const statusStyles: Record<string, string> = {
  Pending: "bg-[#fff3dd] text-[#a56613] ring-[#efd6a8]",
  "In progress": "bg-[#eaf2ff] text-[#35619e] ring-[#cbdcf6]",
  Ready: "bg-[#e6f5ee] text-[#327259] ring-[#c6e6d7]",
  Served: "bg-[#f1efeb] text-[#746d63] ring-[#dfdad2]",
  Cancelled: "bg-[#fdebe7] text-[#ad4e3d] ring-[#f3cfc7]",
};

export function OrderStatusBadge({ status }: { status: number | string }) {
  const label = getOrderStatus(status);
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[0.62rem] font-semibold ring-1 ring-inset ${statusStyles[label] ?? statusStyles.Pending}`}>
      <span className="h-1.5 w-1.5 rounded-full bg-current opacity-80" />
      {label}
    </span>
  );
}

const avatarColors = ["bg-[#d9a24d]", "bg-[#5e8e7d]", "bg-[#d77657]", "bg-[#7c76a8]", "bg-[#56829d]", "bg-[#bd6b87]"];

export function InitialsAvatar({ name, index = 0, className = "h-9 w-9" }: { name: string; index?: number; className?: string }) {
  const initials = name.split(" ").slice(0, 2).map((part) => part[0]).join("");
  return <span className={`flex shrink-0 items-center justify-center rounded-xl text-[0.62rem] font-semibold text-white ${avatarColors[index % avatarColors.length]} ${className}`}>{initials}</span>;
}

export function LoadingBlocks({ count = 4 }: { count?: number }) {
  return (
    <div className="grid gap-3">
      {Array.from({ length: count }).map((_, index) => <div key={index} className="h-16 animate-pulse rounded-xl bg-[#f1eee8]" />)}
    </div>
  );
}
