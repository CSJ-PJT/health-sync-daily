package com.example.fifthdawn;

import android.graphics.Color;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import com.getcapacitor.BridgeActivity;

public class MainActivity extends BridgeActivity {
    private View bootOverlay;
    private View nativeDebugOverlay;

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
            overlay.setOnClickListener(v -> dismissNativeDebugOverlay());
            nativeDebugOverlay = overlay;
            new Handler(Looper.getMainLooper()).postDelayed(this::dismissNativeDebugOverlay, 2400);
        }

        attachBootOverlay();
    }

    private void attachBootOverlay() {
        FrameLayout decor = (FrameLayout) getWindow().getDecorView();

        LinearLayout panel = new LinearLayout(this);
        panel.setOrientation(LinearLayout.VERTICAL);
        panel.setGravity(Gravity.CENTER);
        panel.setBackgroundColor(Color.parseColor("#D907111F"));
        panel.setClickable(true);
        panel.setFocusable(true);

        TextView title = new TextView(this);
        title.setText("Deep Stake");
        title.setTextColor(Color.parseColor("#E8FFF3"));
        title.setTextSize(TypedValue.COMPLEX_UNIT_SP, 28);
        title.setGravity(Gravity.CENTER);

        TextView subtitle = new TextView(this);
        subtitle.setText("앱이 실행 중입니다.\n초기 셸과 월드 표시가 안정될 때까지 짧게 안내를 보여줍니다.");
        subtitle.setTextColor(Color.parseColor("#D7E6F3"));
        subtitle.setTextSize(TypedValue.COMPLEX_UNIT_SP, 16);
        subtitle.setGravity(Gravity.CENTER);
        subtitle.setPadding(dp(24), dp(16), dp(24), 0);

        TextView hint = new TextView(this);
        hint.setText("화면을 누르면 바로 닫을 수 있습니다.");
        hint.setTextColor(Color.parseColor("#9FD7C3"));
        hint.setTextSize(TypedValue.COMPLEX_UNIT_SP, 13);
        hint.setGravity(Gravity.CENTER);
        hint.setPadding(dp(24), dp(20), dp(24), 0);

        panel.addView(title, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        panel.addView(subtitle, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        panel.addView(hint, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.MATCH_PARENT,
            ViewGroup.LayoutParams.MATCH_PARENT
        );
        params.gravity = Gravity.CENTER;
        decor.addView(panel, params);
        bootOverlay = panel;

        panel.setOnClickListener(v -> dismissBootOverlay());
        new Handler(Looper.getMainLooper()).postDelayed(this::dismissBootOverlay, 1200);
    }

    private void dismissBootOverlay() {
        if (bootOverlay == null) {
            return;
        }

        ViewGroup parent = (ViewGroup) bootOverlay.getParent();
        if (parent != null) {
            parent.removeView(bootOverlay);
        }
        bootOverlay = null;
    }

    private void dismissNativeDebugOverlay() {
        if (nativeDebugOverlay == null) {
            return;
        }

        nativeDebugOverlay.animate()
            .alpha(0f)
            .setDuration(220)
            .withEndAction(() -> {
                if (nativeDebugOverlay != null) {
                    nativeDebugOverlay.setVisibility(View.GONE);
                    nativeDebugOverlay.setAlpha(1f);
                }
            })
            .start();
    }

    private int dp(int value) {
        return Math.round(TypedValue.applyDimension(
            TypedValue.COMPLEX_UNIT_DIP,
            value,
            getResources().getDisplayMetrics()
        ));
    }
}
