"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ActionForm } from "@/components/data/action-form";
import { updatePetFieldsAction } from "@/lib/players/pet-actions";
import type { PetSource } from "@/lib/game-json/pet-json";

export interface PetEditableStats {
  name: string | null;
  lvl: number;
  star: number;
  exp: number;
  str: number;
  agi: number;
  _int: number;
  hp: number;
  mp: number;
  maxHp: number;
  maxMp: number;
  tiemnang_point: number;
  skillPoint: number;
}

/** Form sửa "chỉ số cơ bản" của 1 pet — không đụng equip/tatto/skill. */
export function PetEditForm({
  playerId,
  petId,
  source,
  expectedMd5,
  stats,
}: {
  playerId: number;
  petId: number;
  source: PetSource;
  expectedMd5: string;
  stats: PetEditableStats;
}) {
  const [open, setOpen] = useState(false);
  const f = (name: string) => `${name}-${source}-${petId}`;

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          Sửa
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Sửa pet #{petId}</DialogTitle>
        </DialogHeader>
        <ActionForm action={updatePetFieldsAction} submitLabel="Lưu" onSuccess={() => setOpen(false)} className="space-y-3">
          <input type="hidden" name="playerId" value={playerId} />
          <input type="hidden" name="petId" value={petId} />
          <input type="hidden" name="source" value={source} />
          <input type="hidden" name="expectedMd5" value={expectedMd5} />
          <div className="grid grid-cols-2 gap-3">
            <div className="col-span-2 space-y-1">
              <Label htmlFor={f("name")}>Tên (để trống = dùng tên loài mặc định)</Label>
              <Input id={f("name")} name="name" defaultValue={stats.name ?? ""} maxLength={64} />
            </div>
            <Field label="Cấp (lvl)" name="lvl" id={f("lvl")} defaultValue={stats.lvl} min={1} />
            <Field label="Sao (0-5)" name="star" id={f("star")} defaultValue={stats.star} min={0} max={5} />
            <Field label="Exp" name="exp" id={f("exp")} defaultValue={stats.exp} min={0} />
            <Field label="Điểm tiềm năng" name="tiemnang_point" id={f("tiemnang_point")} defaultValue={stats.tiemnang_point} min={0} />
            <Field label="Sức mạnh (str)" name="str" id={f("str")} defaultValue={stats.str} min={0} />
            <Field label="Nhanh nhẹn (agi)" name="agi" id={f("agi")} defaultValue={stats.agi} min={0} />
            <Field label="Trí tuệ (int)" name="_int" id={f("int")} defaultValue={stats._int} min={0} />
            <Field label="Điểm kỹ năng" name="skillPoint" id={f("skillPoint")} defaultValue={stats.skillPoint} min={0} />
            <Field label="HP" name="hp" id={f("hp")} defaultValue={stats.hp} min={0} />
            <Field label="HP tối đa" name="maxHp" id={f("maxHp")} defaultValue={stats.maxHp} min={1} />
            <Field label="MP" name="mp" id={f("mp")} defaultValue={stats.mp} min={0} />
            <Field label="MP tối đa" name="maxMp" id={f("maxMp")} defaultValue={stats.maxMp} min={1} />
          </div>
        </ActionForm>
      </DialogContent>
    </Dialog>
  );
}

function Field({
  label,
  name,
  id,
  defaultValue,
  min,
  max,
}: {
  label: string;
  name: string;
  id: string;
  defaultValue: number;
  min?: number;
  max?: number;
}) {
  return (
    <div className="space-y-1">
      <Label htmlFor={id}>{label}</Label>
      <Input id={id} name={name} type="number" min={min} max={max} defaultValue={defaultValue} required />
    </div>
  );
}
