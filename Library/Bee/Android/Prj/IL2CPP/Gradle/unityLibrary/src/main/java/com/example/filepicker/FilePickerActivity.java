package com.example.filepicker;

import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.File;

public class FilePickerActivity extends Activity {

    private static final int PICK_FILE_REQUEST = 1;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("*/*"); // 모든 파일 유형 허용
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        startActivityForResult(intent, PICK_FILE_REQUEST);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == PICK_FILE_REQUEST && resultCode == RESULT_OK) {
            Uri uri = data.getData();
            String path = getRealPathFromURI(uri);

            if (path != null) {
                UnityPlayer.UnitySendMessage("FileUploader", "OnFileSelected", path);
            } else {
                Log.e("FilePicker", "Failed to retrieve file path");
                UnityPlayer.UnitySendMessage("FileUploader", "OnFileSelected", "");
            }
        } else {
            UnityPlayer.UnitySendMessage("FileUploader", "OnFileSelected", "");
        }

        finish();
    }

    /**
     * URI를 실제 파일 경로로 변환
     */
    private String getRealPathFromURI(Uri uri) {
        String path = null;

        // content:// URI 처리
        if ("content".equalsIgnoreCase(uri.getScheme())) {
            String[] projection = {MediaStore.MediaColumns.DATA};
            Cursor cursor = null;
            try {
                cursor = getContentResolver().query(uri, projection, null, null, null);
                if (cursor != null && cursor.moveToFirst()) {
                    int columnIndex = cursor.getColumnIndexOrThrow(MediaStore.MediaColumns.DATA);
                    path = cursor.getString(columnIndex);
                }
            } catch (Exception e) {
                Log.e("FilePicker", "Error getting file path: " + e.getMessage());
            } finally {
                if (cursor != null) {
                    cursor.close();
                }
            }
        }

        // file:// URI 처리
        else if ("file".equalsIgnoreCase(uri.getScheme())) {
            path = uri.getPath();
        }

        // 위 두 가지 방식으로도 경로를 얻지 못했을 경우 null 반환
        return path;
    }
}