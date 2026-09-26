"use client";

import type { ReactNode } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

/**
 * Nội dung mỗi tab được render ở SERVER (RSC) và truyền vào như children — component này chỉ
 * lo việc chuyển tab phía client, không tự fetch gì thêm.
 */
export function PlayerDetailTabs({
  overview,
  inventory,
  pets,
  letters,
  friends,
  quests,
  raw,
}: {
  overview: ReactNode;
  /** Tab "Vật phẩm" — sửa/xoá item có sẵn (phase 6). */
  inventory: ReactNode;
  /** Tab "Pet" — sửa chỉ số cơ bản pet (phase 6). */
  pets: ReactNode;
  letters: ReactNode;
  friends: ReactNode;
  quests: ReactNode;
  raw: ReactNode;
}) {
  return (
    <Tabs defaultValue="overview">
      <TabsList>
        <TabsTrigger value="overview">Tổng quan</TabsTrigger>
        <TabsTrigger value="inventory">Vật phẩm</TabsTrigger>
        <TabsTrigger value="pets">Pet</TabsTrigger>
        <TabsTrigger value="letters">Thư</TabsTrigger>
        <TabsTrigger value="friends">Bạn bè</TabsTrigger>
        <TabsTrigger value="quests">Nhiệm vụ/thành tựu</TabsTrigger>
        <TabsTrigger value="raw">Raw JSON</TabsTrigger>
      </TabsList>
      <TabsContent value="overview">{overview}</TabsContent>
      <TabsContent value="inventory">{inventory}</TabsContent>
      <TabsContent value="pets">{pets}</TabsContent>
      <TabsContent value="letters">{letters}</TabsContent>
      <TabsContent value="friends">{friends}</TabsContent>
      <TabsContent value="quests">{quests}</TabsContent>
      <TabsContent value="raw">{raw}</TabsContent>
    </Tabs>
  );
}
