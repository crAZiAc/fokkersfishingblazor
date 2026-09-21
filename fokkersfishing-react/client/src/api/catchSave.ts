import { api } from './client';
import { uploadPhoto, deletePhotos } from './photos';
import { PhotoType, type Catch } from './types';
import type { CatchSaveResult } from '../components/EditCatchDialog';

async function applyPhotos(result: CatchSaveResult): Promise<Catch> {
  const c = { ...result.updated };
  if (result.measureFile) {
    const res = await uploadPhoto(c.id, PhotoType.Measure, result.measureFile);
    c.measurePhotoUrl = res.photoUrl;
    c.measureThumbnailUrl = res.thumbnailUrl;
  }
  if (result.catchFile) {
    const res = await uploadPhoto(c.id, PhotoType.Catch, result.catchFile);
    c.catchPhotoUrl = res.photoUrl;
    c.catchThumbnailUrl = res.thumbnailUrl;
  }
  if (result.clearPhotos) {
    c.catchPhotoUrl = '';
    c.catchThumbnailUrl = '';
    c.measurePhotoUrl = '';
    c.measureThumbnailUrl = '';
  }
  return c;
}

/** Create a new catch (user or admin "add" flow — both POST /catch). */
export async function createCatch(result: CatchSaveResult): Promise<Catch> {
  const c = await applyPhotos(result);
  const { data } = await api.post<Catch>('/catch', c);
  return data;
}

/** Update a catch via the user/team endpoint (resets status to Pending server-side). */
export async function updateUserCatch(result: CatchSaveResult): Promise<Catch> {
  const c = await applyPhotos(result);
  const { data } = await api.put<Catch>(`/catch/${c.id}`, c);
  if (result.clearPhotos) await deletePhotos(c.id);
  return data;
}

/** Quick admin update (status + core fields) via the admin endpoint. */
export async function updateAdminCatch(result: CatchSaveResult): Promise<Catch> {
  const c = await applyPhotos(result);
  const { data } = await api.put<Catch>(`/catch/admin/${c.id}`, c);
  if (result.clearPhotos) await deletePhotos(c.id);
  return data;
}

export async function deleteUserCatch(id: string): Promise<void> {
  await api.delete(`/catch/${id}`);
  await deletePhotos(id);
}

export async function deleteTeamCatch(id: string): Promise<void> {
  await api.delete(`/catch/team/${id}`);
  await deletePhotos(id);
}

export async function deleteAdminCatch(id: string): Promise<void> {
  await api.delete(`/catch/admin/${id}`);
  await deletePhotos(id);
}
