package com.example.fifthdawn;

import android.graphics.Color;
import android.os.Bundle;
import android.view.View;
import android.webkit.WebView;
import com.getcapacitor.BridgeActivity;

public class MainActivity extends BridgeActivity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        WebView.setWebContentsDebuggingEnabled(true);

        WebView webView = findViewById(com.getcapacitor.android.R.id.webview);
        if (webView == null) {
            webView = findViewById(getResources().getIdentifier("webview", "id", getPackageName()));
        }

        if (webView != null) {
            webView.setBackgroundColor(Color.parseColor("#07111F"));
        }

        View overlay = findViewById(getResources().getIdentifier("native_debug_overlay", "id", getPackageName()));
        if (overlay != null) {
            overlay.bringToFront();
        }
    }
}
