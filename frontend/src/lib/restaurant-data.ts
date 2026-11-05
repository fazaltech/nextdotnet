export type DashboardSummary = {
  pendingOrders: number;
  todaySales: number;
  totalCustomers: number;
  totalMenuItems: number;
  lowStockItems: number;
  todayExpenses: number;
};

export type OrderItem = {
  id: number;
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
};

export type RestaurantOrder = {
  id: number;
  tableId: number;
  tableNumber: string;
  customerId: number | null;
  customerName: string | null;
  orderDateUtc: string;
  status: number | string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  items: OrderItem[];
};

export type MenuItem = {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  description: string | null;
  price: number;
  isAvailable: boolean;
  isActive: boolean;
};

export type DiningTable = {
  id: number;
  tableNumber: string;
  capacity: number;
  isOccupied: boolean;
  isActive: boolean;
};

export type InventoryItem = {
  id: number;
  name: string;
  unit: string;
  quantity: number;
  reorderLevel: number;
  unitCost: number;
  isActive: boolean;
};

export type Customer = {
  id: number;
  fullName: string;
  phone: string | null;
  email: string | null;
  isActive: boolean;
};

export function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: value % 1 === 0 ? 0 : 2,
  }).format(value);
}

const statusNames: Record<number, string> = {
  1: "Pending",
  2: "In progress",
  3: "Ready",
  4: "Served",
  5: "Cancelled",
};

export function getOrderStatus(status: number | string): string {
  if (typeof status === "number") return statusNames[status] ?? "Pending";
  return status.replace(/([a-z])([A-Z])/g, "$1 $2");
}

export function minutesAgo(date: string): string {
  const minutes = Math.max(1, Math.round((Date.now() - new Date(date).getTime()) / 60_000));
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  return `${hours}h ago`;
}
