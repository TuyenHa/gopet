"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { Menu, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { NAV_GROUPS, findActiveHref } from "./sidebar-nav-config";

function NavList({ collapsed, onNavigate }: { collapsed: boolean; onNavigate?: () => void }) {
  const active = findActiveHref(usePathname());
  return (
    <nav className="flex flex-col gap-4 px-2 py-4">
      {NAV_GROUPS.map((g) => (
        <div key={g.label}>
          {!collapsed && (
            <div className="px-3 pb-1 text-xs font-medium uppercase tracking-wide text-neutral-400">{g.label}</div>
          )}
          <ul className="space-y-0.5">
            {g.items.map((it) => {
              const link = (
                <Link
                  href={it.href}
                  onClick={onNavigate}
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm text-neutral-600 hover:bg-neutral-50 hover:text-neutral-900",
                    active === it.href && "bg-neutral-100 font-medium text-neutral-900",
                    collapsed && "justify-center px-2",
                  )}
                >
                  <it.icon className="size-4 shrink-0" />
                  {!collapsed && <span className="truncate">{it.label}</span>}
                </Link>
              );
              return (
                <li key={it.href}>
                  {collapsed ? (
                    <Tooltip>
                      <TooltipTrigger asChild>{link}</TooltipTrigger>
                      <TooltipContent side="right">{it.label}</TooltipContent>
                    </Tooltip>
                  ) : (
                    link
                  )}
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </nav>
  );
}

/** Sidebar cố định bên trái (≥ md), thu gọn được về dạng icon. */
export function AppSidebar() {
  const [collapsed, setCollapsed] = useState(false);
  return (
    <aside
      className={cn(
        "sticky top-0 hidden h-screen shrink-0 flex-col overflow-y-auto border-r bg-white md:flex",
        collapsed ? "w-16" : "w-60",
      )}
    >
      <div className={cn("flex h-14 items-center border-b px-4", collapsed ? "justify-center px-2" : "justify-between")}>
        {!collapsed && (
          <Link href="/" className="font-semibold">
            Gopet Admin
          </Link>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setCollapsed((c) => !c)}
          aria-label={collapsed ? "Mở rộng menu" : "Thu gọn menu"}
        >
          {collapsed ? <PanelLeftOpen className="size-4" /> : <PanelLeftClose className="size-4" />}
        </Button>
      </div>
      <NavList collapsed={collapsed} />
    </aside>
  );
}

/** Nút mở menu dạng Sheet trượt trên màn hình nhỏ (< md). */
export function MobileSidebar() {
  const [open, setOpen] = useState(false);
  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" className="md:hidden" aria-label="Mở menu">
          <Menu className="size-5" />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-64 overflow-y-auto bg-white p-0">
        <SheetTitle className="flex h-14 items-center border-b px-4 font-semibold">Gopet Admin</SheetTitle>
        <NavList collapsed={false} onNavigate={() => setOpen(false)} />
      </SheetContent>
    </Sheet>
  );
}
