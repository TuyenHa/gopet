import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { JsonViewer } from "@/components/data/json-viewer";
import type { PlayerQuestsSnapshot } from "@/lib/players/player-detail-queries";

export function QuestsReadonlyTab({ data }: { data: PlayerQuestsSnapshot }) {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Nhiệm vụ đang làm (tasking) / đã xong (task)</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <JsonViewer value={data.tasking} maxHeight={200} />
          <JsonViewer value={data.task} maxHeight={200} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Thành tựu</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2 text-sm">
          <p>Thành tựu hiện tại: {data.CurrentAchievementId < 0 ? "không có" : data.CurrentAchievementId}</p>
          <p>Điểm tích luỹ: {data.AccumulatedPoint}</p>
          <JsonViewer value={data.achievements} />
        </CardContent>
      </Card>
    </div>
  );
}
