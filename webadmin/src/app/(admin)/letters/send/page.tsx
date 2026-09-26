import { PageHeader } from "@/components/data/page-header";
import { SendAllLetterForm } from "@/components/letters/send-all-letter-form";
import { SendOneLetterForm } from "@/components/letters/send-one-letter-form";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { requireAdmin } from "@/lib/auth/require-admin";
import { firstParam } from "@/lib/pagination";

/** `?targetUserId=` (phase 5 — thư từ trang nhân vật): điền sẵn tab "gửi 1 người". */
export default async function SendLetterPage({ searchParams }: PageProps<"/letters/send">) {
  await requireAdmin();
  const sp = await searchParams;
  const targetUserId = firstParam(sp.targetUserId);

  return (
    <div className="max-w-2xl space-y-4">
      <PageHeader title="Gửi thư hệ thống" description="Thư vào hàng đợi, giao khi người nhận đăng nhập lần kế tiếp." />
      <Tabs defaultValue="one">
        <TabsList>
          <TabsTrigger value="one">Gửi 1 người</TabsTrigger>
          <TabsTrigger value="all">Gửi tất cả</TabsTrigger>
        </TabsList>
        <TabsContent value="one">
          <SendOneLetterForm defaultTarget={targetUserId} />
        </TabsContent>
        <TabsContent value="all">
          <SendAllLetterForm />
        </TabsContent>
      </Tabs>
    </div>
  );
}
