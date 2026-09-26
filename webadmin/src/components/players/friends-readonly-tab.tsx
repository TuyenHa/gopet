import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { JsonViewer } from "@/components/data/json-viewer";
import type { PlayerFriendsSnapshot } from "@/lib/players/player-detail-queries";

export function FriendsReadonlyTab({ data }: { data: PlayerFriendsSnapshot }) {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
      <Card>
        <CardHeader>
          <CardTitle>Danh sách bạn bè</CardTitle>
        </CardHeader>
        <CardContent>
          <JsonViewer value={data.ListFriends} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Lời mời kết bạn</CardTitle>
        </CardHeader>
        <CardContent>
          <JsonViewer value={data.RequestAddFriends} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Đã chặn</CardTitle>
        </CardHeader>
        <CardContent>
          <JsonViewer value={data.BlockFriendLists} />
        </CardContent>
      </Card>
    </div>
  );
}
