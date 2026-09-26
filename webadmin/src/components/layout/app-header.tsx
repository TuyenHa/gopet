import { ChevronDown, LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { logoutAction } from "@/lib/auth/logout-action";
import { MobileSidebar } from "./app-sidebar";

export function AppHeader({ playerName, username, isSuperAdmin }: { playerName: string; username: string; isSuperAdmin: boolean }) {
  return (
    <header className="sticky top-0 z-10 flex h-14 items-center gap-2 border-b bg-white px-4">
      <MobileSidebar />
      <div className="flex-1" />
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="gap-2">
            <span className="font-medium">{playerName}</span>
            {isSuperAdmin && <span className="rounded bg-blue-50 px-1.5 py-0.5 text-xs text-blue-700">super</span>}
            <ChevronDown className="size-4 text-neutral-400" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-48">
          <DropdownMenuLabel className="font-normal text-neutral-500">Tài khoản: {username}</DropdownMenuLabel>
          <DropdownMenuSeparator />
          <form action={logoutAction}>
            <DropdownMenuItem asChild>
              <button type="submit" className="w-full">
                <LogOut className="size-4" /> Đăng xuất
              </button>
            </DropdownMenuItem>
          </form>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  );
}
