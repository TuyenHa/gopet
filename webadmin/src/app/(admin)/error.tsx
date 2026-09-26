"use client";

import { Button } from "@/components/ui/button";

export default function AdminError({ error, reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <div className="mx-auto max-w-md rounded-lg border border-red-200 bg-red-50 p-6 text-center">
      <h2 className="font-semibold text-red-800">Không tải được trang</h2>
      <p className="mt-2 text-sm text-red-700">
        Có lỗi khi đọc dữ liệu (kiểm tra kết nối database).{error.digest && ` Mã lỗi: ${error.digest}`}
      </p>
      <Button variant="outline" className="mt-4" onClick={reset}>
        Thử lại
      </Button>
    </div>
  );
}
