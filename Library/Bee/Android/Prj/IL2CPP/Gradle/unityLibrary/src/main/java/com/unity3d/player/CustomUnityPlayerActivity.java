package com.unity3d.player;

import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.provider.MediaStore;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {
    @Override
    protected void onPause() {
        super.onPause();
        if (mUnityPlayer != null) {
            mUnityPlayer.pause();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (mUnityPlayer != null) {
            mUnityPlayer.resume();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == 1 && resultCode == RESULT_OK && data != null) {
            try {
                Uri uri = data.getData();
                String path = getPathFromUri(uri);

                UnityPlayer.UnitySendMessage("MobileFileBrowserHandler", "OnFileSelected", path);
            } catch (Exception e) {
                UnityPlayer.UnitySendMessage("MobileFileBrowserHandler", "OnFileSelected", "");
            }
        }
    }

private String getPathFromUri(Uri uri) {
    Cursor cursor = null;
    try {
        String documentId;

        // 1. 오디오 파일 처리: documentId 기반 접근
        if (uri.toString().contains("audio")) {
            cursor = getContentResolver().query(uri, null, null, null, null);
            if (cursor != null && cursor.moveToFirst()) {
                documentId = cursor.getString(0).split(":")[1];
                cursor.close();

                cursor = getContentResolver().query(
                    MediaStore.Files.getContentUri("external"),
                    null,
                    MediaStore.Files.FileColumns._ID + " = ? ",
                    new String[]{documentId},
                    null
                );

                if (cursor != null && cursor.moveToFirst()) {
                    String path = cursor.getString(cursor.getColumnIndex(MediaStore.Files.FileColumns.DATA));
                    cursor.close();
                    return path; // 오디오 파일 경로 반환
                }
            }
        } 
        // 2. 이미지 파일 처리
        else if (uri.toString().contains("image")) {
            String[] projection = {MediaStore.Images.Media.DATA};
            cursor = getContentResolver().query(uri, projection, null, null, null);

            if (cursor != null && cursor.moveToFirst()) {
                int columnIndex = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
                String path = cursor.getString(columnIndex);
                cursor.close();
                return path; // 이미지 파일 경로 반환
            }
        }
    } catch (Exception e) {
        e.printStackTrace();
    } finally {
        if (cursor != null) {
            cursor.close();
        }
    }
    return null; // 실패 시 null 반환
}




}
