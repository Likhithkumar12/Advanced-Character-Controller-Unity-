package otpreader;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.auth.api.phone.SmsRetriever;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.Status;

public class SmsBroadcastReceiver extends BroadcastReceiver {
    private static final String TAG = "SmsBroadcastReceiver";

    public interface OTPListener {
        void onOTPReceived(String otp);
        void onOTPTimeout();
    }

    private OTPListener otpListener;

    public void setOTPListener(OTPListener listener) {
        this.otpListener = listener;
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        if (SmsRetriever.SMS_RETRIEVED_ACTION.equals(intent.getAction())) {
            Bundle extras = intent.getExtras();
            if (extras == null) {
                return;
            }

            Status status = (Status) extras.get(SmsRetriever.EXTRA_STATUS);
            if (status == null) {
                return;
            }

            switch (status.getStatusCode()) {
                case CommonStatusCodes.SUCCESS:
                    String message = (String) extras.get(SmsRetriever.EXTRA_SMS_MESSAGE);
                    
                    if (message != null && otpListener != null) {
                        String otp = extractOtpFromMessage(message);
                        if (!otp.isEmpty()) {
                            otpListener.onOTPReceived(otp);
                        }
                    }
                    break;

                case CommonStatusCodes.TIMEOUT:
                    if (otpListener != null) {
                        otpListener.onOTPTimeout();
                    }
                    break;
            }
        }
    }

    private String extractOtpFromMessage(String message) {
        // Extract 4-8 digit codes (common OTP lengths)
        java.util.regex.Pattern pattern = java.util.regex.Pattern.compile("\\b\\d{4,8}\\b");
        java.util.regex.Matcher matcher = pattern.matcher(message);
        
        if (matcher.find()) {
            return matcher.group();
        }
        
        // Fallback: Extract all digits and return first 6
        String allDigits = message.replaceAll("\\D+", "");
        if (allDigits.length() >= 4) {
            return allDigits.substring(0, Math.min(6, allDigits.length()));
        }
        
        return "";
    }
}
