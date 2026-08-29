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

export const dashboardFallback: DashboardSummary = {
  pendingOrders: 12,
  todaySales: 4826.4,
  totalCustomers: 1268,
  totalMenuItems: 48,
  lowStockItems: 5,
  todayExpenses: 842.15,
};

export const orderFallback: RestaurantOrder[] = [
  {
    id: 1048,
    tableId: 4,
    tableNumber: "T-04",
    customerId: 1,
    customerName: "Ava Morgan",
    orderDateUtc: new Date(Date.now() - 8 * 60_000).toISOString(),
    status: 2,
    subtotal: 72,
    taxAmount: 5.76,
    totalAmount: 77.76,
    items: [
      { id: 1, menuItemId: 1, menuItemName: "Truffle Rigatoni", quantity: 2, unitPrice: 24, lineTotal: 48 },
      { id: 2, menuItemId: 3, menuItemName: "Citrus Garden Salad", quantity: 1, unitPrice: 16, lineTotal: 16 },
      { id: 3, menuItemId: 7, menuItemName: "Sparkling Water", quantity: 2, unitPrice: 4, lineTotal: 8 },
    ],
  },
  {
    id: 1047,
    tableId: 11,
    tableNumber: "T-11",
    customerId: 2,
    customerName: "Noah Williams",
    orderDateUtc: new Date(Date.now() - 14 * 60_000).toISOString(),
    status: 3,
    subtotal: 54,
    taxAmount: 4.32,
    totalAmount: 58.32,
    items: [
      { id: 4, menuItemId: 5, menuItemName: "Fire-Grilled Salmon", quantity: 1, unitPrice: 32, lineTotal: 32 },
      { id: 5, menuItemId: 8, menuItemName: "Miso Corn Ribs", quantity: 1, unitPrice: 14, lineTotal: 14 },
      { id: 6, menuItemId: 7, menuItemName: "House Lemonade", quantity: 1, unitPrice: 8, lineTotal: 8 },
    ],
  },
  {
    id: 1046,
    tableId: 7,
    tableNumber: "T-07",
    customerId: null,
    customerName: "Walk-in",
    orderDateUtc: new Date(Date.now() - 21 * 60_000).toISOString(),
    status: 1,
    subtotal: 38,
    taxAmount: 3.04,
    totalAmount: 41.04,
    items: [
      { id: 7, menuItemId: 2, menuItemName: "Smash Burger", quantity: 1, unitPrice: 20, lineTotal: 20 },
      { id: 8, menuItemId: 6, menuItemName: "Rosemary Fries", quantity: 1, unitPrice: 10, lineTotal: 10 },
      { id: 9, menuItemId: 7, menuItemName: "Iced Tea", quantity: 1, unitPrice: 8, lineTotal: 8 },
    ],
  },
  {
    id: 1045,
    tableId: 2,
    tableNumber: "T-02",
    customerId: 4,
    customerName: "Sophia Chen",
    orderDateUtc: new Date(Date.now() - 34 * 60_000).toISOString(),
    status: 4,
    subtotal: 96,
    taxAmount: 7.68,
    totalAmount: 103.68,
    items: [
      { id: 10, menuItemId: 4, menuItemName: "Slow Braised Short Rib", quantity: 2, unitPrice: 38, lineTotal: 76 },
      { id: 11, menuItemId: 9, menuItemName: "Chocolate Torte", quantity: 2, unitPrice: 10, lineTotal: 20 },
    ],
  },
  {
    id: 1044,
    tableId: 9,
    tableNumber: "T-09",
    customerId: 5,
    customerName: "Mateo Silva",
    orderDateUtc: new Date(Date.now() - 51 * 60_000).toISOString(),
    status: 4,
    subtotal: 66,
    taxAmount: 5.28,
    totalAmount: 71.28,
    items: [
      { id: 12, menuItemId: 5, menuItemName: "Fire-Grilled Salmon", quantity: 1, unitPrice: 32, lineTotal: 32 },
      { id: 13, menuItemId: 3, menuItemName: "Citrus Garden Salad", quantity: 1, unitPrice: 16, lineTotal: 16 },
      { id: 14, menuItemId: 10, menuItemName: "Basque Cheesecake", quantity: 1, unitPrice: 12, lineTotal: 12 },
      { id: 15, menuItemId: 7, menuItemName: "Espresso", quantity: 2, unitPrice: 3, lineTotal: 6 },
    ],
  },
];

export const menuFallback: MenuItem[] = [
  { id: 1, categoryId: 1, categoryName: "Mains", name: "Truffle Rigatoni", description: "Wild mushrooms, pecorino and black truffle cream.", price: 24, isAvailable: true, isActive: true },
  { id: 2, categoryId: 1, categoryName: "Mains", name: "Ember Smash Burger", description: "Double beef, smoked cheddar, pickles and house sauce.", price: 20, isAvailable: true, isActive: true },
  { id: 3, categoryId: 2, categoryName: "Starters", name: "Citrus Garden Salad", description: "Baby greens, orange, fennel, pistachio and chèvre.", price: 16, isAvailable: true, isActive: true },
  { id: 4, categoryId: 1, categoryName: "Mains", name: "Braised Short Rib", description: "Twelve-hour beef, silky potato and red wine jus.", price: 38, isAvailable: true, isActive: true },
  { id: 5, categoryId: 1, categoryName: "Mains", name: "Fire-Grilled Salmon", description: "Herb couscous, charred lemon and beurre blanc.", price: 32, isAvailable: true, isActive: true },
  { id: 6, categoryId: 2, categoryName: "Sides", name: "Rosemary Fries", description: "Crisp potatoes, rosemary salt and roasted garlic aioli.", price: 10, isAvailable: true, isActive: true },
  { id: 7, categoryId: 3, categoryName: "Drinks", name: "House Lemonade", description: "Meyer lemon, fresh mint and sparkling water.", price: 8, isAvailable: true, isActive: true },
  { id: 8, categoryId: 2, categoryName: "Starters", name: "Miso Corn Ribs", description: "Charred corn, white miso butter and shichimi.", price: 14, isAvailable: false, isActive: true },
  { id: 9, categoryId: 4, categoryName: "Desserts", name: "Chocolate Torte", description: "Dark chocolate, cocoa nib and crème fraîche.", price: 10, isAvailable: true, isActive: true },
  { id: 10, categoryId: 4, categoryName: "Desserts", name: "Basque Cheesecake", description: "Burnt honey, vanilla bean and sea salt.", price: 12, isAvailable: true, isActive: true },
];

export const tableFallback: DiningTable[] = Array.from({ length: 16 }, (_, index) => ({
  id: index + 1,
  tableNumber: `T-${String(index + 1).padStart(2, "0")}`,
  capacity: [2, 4, 4, 6, 2, 4, 4, 8][index % 8],
  isOccupied: [1, 2, 4, 7, 9, 11, 14].includes(index + 1),
  isActive: index !== 15,
}));

export const inventoryFallback: InventoryItem[] = [
  { id: 1, name: "Atlantic salmon", unit: "kg", quantity: 4.2, reorderLevel: 5, unitCost: 18.5, isActive: true },
  { id: 2, name: "Beef short rib", unit: "kg", quantity: 9.8, reorderLevel: 6, unitCost: 15.2, isActive: true },
  { id: 3, name: "Heavy cream", unit: "L", quantity: 3, reorderLevel: 4, unitCost: 4.1, isActive: true },
  { id: 4, name: "Wild mushrooms", unit: "kg", quantity: 2.1, reorderLevel: 3, unitCost: 12.8, isActive: true },
  { id: 5, name: "Potatoes", unit: "kg", quantity: 18.4, reorderLevel: 8, unitCost: 1.7, isActive: true },
  { id: 6, name: "Baby greens", unit: "kg", quantity: 1.4, reorderLevel: 2.5, unitCost: 9.4, isActive: true },
  { id: 7, name: "Restaurant flour", unit: "kg", quantity: 24, reorderLevel: 10, unitCost: 1.2, isActive: true },
  { id: 8, name: "Cooking oil", unit: "L", quantity: 11, reorderLevel: 7, unitCost: 3.6, isActive: true },
];

export const customerFallback: Customer[] = [
  { id: 1, fullName: "Ava Morgan", phone: "+1 212 555 0182", email: "ava.morgan@example.com", isActive: true },
  { id: 2, fullName: "Noah Williams", phone: "+1 646 555 0128", email: "noah.w@example.com", isActive: true },
  { id: 3, fullName: "Emma Johnson", phone: "+1 917 555 0164", email: "emma.j@example.com", isActive: true },
  { id: 4, fullName: "Sophia Chen", phone: "+1 718 555 0149", email: "sophia.chen@example.com", isActive: true },
  { id: 5, fullName: "Mateo Silva", phone: "+1 347 555 0197", email: "mateo.s@example.com", isActive: true },
  { id: 6, fullName: "Olivia Brown", phone: "+1 929 555 0135", email: "olivia.b@example.com", isActive: false },
];

export const weeklySales = [
  { day: "Mon", sales: 3180, orders: 68 },
  { day: "Tue", sales: 3620, orders: 74 },
  { day: "Wed", sales: 3350, orders: 71 },
  { day: "Thu", sales: 4290, orders: 86 },
  { day: "Fri", sales: 5680, orders: 112 },
  { day: "Sat", sales: 6240, orders: 128 },
  { day: "Sun", sales: 4826, orders: 96 },
];

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
