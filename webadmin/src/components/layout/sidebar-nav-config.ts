import {
  Activity,
  Boxes,
  ClipboardList,
  Crown,
  Gift,
  LayoutDashboard,
  Map as MapIcon,
  Mail,
  Package,
  ScrollText,
  Server,
  Settings,
  ShoppingBag,
  Skull,
  Store,
  Swords,
  Table2,
  Users,
  UserSquare,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  label: string;
  href: string;
  icon: LucideIcon;
}

export interface NavGroup {
  label: string;
  items: NavItem[];
}

/** Nguồn DUY NHẤT của menu sidebar (desktop + mobile). */
export const NAV_GROUPS: NavGroup[] = [
  { label: "Tổng quan", items: [{ label: "Dashboard", href: "/", icon: LayoutDashboard }] },
  {
    label: "Người dùng",
    items: [
      { label: "Tài khoản", href: "/accounts", icon: Users },
      { label: "Nhân vật", href: "/players", icon: UserSquare },
    ],
  },
  {
    label: "Dữ liệu game",
    items: [
      { label: "Vật phẩm", href: "/data/item", icon: Package },
      { label: "Shop", href: "/data/shop", icon: ShoppingBag },
      { label: "Shop lôi đài", href: "/data/shoparena", icon: Swords },
      { label: "Rơi đồ", href: "/data/drop_item", icon: Boxes },
      { label: "Boss", href: "/data/boss", icon: Skull },
      { label: "Bản đồ", href: "/data/map", icon: MapIcon },
      { label: "Cài đặt server", href: "/data/field", icon: Settings },
      { label: "Máy chủ", href: "/data/server", icon: Server },
      { label: "Bảng khác", href: "/data", icon: Table2 },
    ],
  },
  {
    label: "Vận hành",
    items: [
      { label: "Giftcode", href: "/giftcodes", icon: Gift },
      { label: "Thư hệ thống", href: "/letters", icon: Mail },
    ],
  },
  {
    label: "Giám sát",
    items: [
      { label: "Lịch sử người chơi", href: "/logs/history", icon: ScrollText },
      { label: "Chợ trời", href: "/market", icon: Store },
      { label: "Bang hội", href: "/clans", icon: Crown },
      { label: "Lịch sử đăng nhập", href: "/logs/logins", icon: Activity },
      { label: "Audit log", href: "/logs/audit", icon: ClipboardList },
    ],
  },
];

/** Mục active: khớp chính xác, hoặc là tiền tố dài nhất (vd /data/item/edit → "Vật phẩm", không phải "Bảng khác"). */
export function findActiveHref(pathname: string): string | undefined {
  let best: string | undefined;
  for (const g of NAV_GROUPS)
    for (const it of g.items) {
      const hit = it.href === "/" ? pathname === "/" : pathname === it.href || pathname.startsWith(it.href + "/");
      if (hit && (!best || it.href.length > best.length)) best = it.href;
    }
  return best;
}
