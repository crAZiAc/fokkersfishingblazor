import { api } from './client';
import { PhotoType, type UploadPhotoResponse } from './types';

function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      // Strip the "data:*/*;base64," prefix — the API expects the raw base64.
      const comma = result.indexOf(',');
      resolve(comma >= 0 ? result.slice(comma + 1) : result);
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

/** Uploads one photo for a catch and returns the stored URLs. */
export async function uploadPhoto(catchId: string, type: PhotoType, file: File): Promise<UploadPhotoResponse> {
  const imageContent = await fileToBase64(file);
  const { data } = await api.post<UploadPhotoResponse>('/photo', {
    id: catchId,
    photoType: type,
    imageContent,
  });
  return data;
}

export async function deletePhotos(catchId: string): Promise<void> {
  try {
    await api.delete(`/photo/${catchId}`);
  } catch {
    /* best-effort */
  }
}
