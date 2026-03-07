package otpreader;

import android.app.Activity;
import android.content.Context; // Import Context
import android.content.IntentFilter;
import android.os.Build; // Import Build
import android.util.Log;

import com.google.android.gms.auth.api.phone.SmsRetriever;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;

import com.unity3d.player.UnityPlayer;

public class OtpPlugin {
    private static final String TAG = "OtpPlugin";
    private Activity activity;
    private SmsBroadcastReceiver smsReceiver;

    public OtpPlugin(Activity activity) {
        this.activity = activity;
    }

    public void StartSmsListener() {
        if (activity == null) {
            Log.e(TAG, "Activity is null in StartSmsListener");
            return;
        }

        try {
            AppHashHelper.getAppHash();
        } catch (Exception e) {
            Log.e(TAG, "Error calling AppHashHelper: " + e.getMessage());
        }

        try {
            Task<Void> task = SmsRetriever.getClient(activity).startSmsRetriever();
            
            task.addOnSuccessListener(new OnSuccessListener<Void>() {
                @Override
                public void onSuccess(Void aVoid) {
                    // Create and register the broadcast receiver
                    smsReceiver = new SmsBroadcastReceiver();
                    smsReceiver.setOTPListener(new SmsBroadcastReceiver.OTPListener() {
                        @Override
                        public void onOTPReceived(String otp) {
                            UnityPlayer.UnitySendMessage("OtpManager", "OnOtpReceived", otp);
                        }

                        @Override
                        public void onOTPTimeout() {
                            UnityPlayer.UnitySendMessage("OtpManager", "OnOtpTimeout", "");
                        }
                    });

                    IntentFilter filter = new IntentFilter(SmsRetriever.SMS_RETRIEVED_ACTION);
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) { // API 33+
                        activity.registerReceiver(smsReceiver, filter, Context.RECEIVER_NOT_EXPORTED);
                        Log.d(TAG, "✅ DEBUG - SMS receiver registered for API 33+ with RECEIVER_NOT_EXPORTED");
                    } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) { // API 26-32 for the overload with more args
                         // For these versions, the flag parameter is the 5th argument in the most common specific overload
                         // public Intent registerReceiver (BroadcastReceiver receiver, IntentFilter filter, String broadcastPermission, Handler scheduler, int flags)
                         activity.registerReceiver(smsReceiver, filter, null, null, Context.RECEIVER_NOT_EXPORTED);
                         Log.d(TAG, "✅ DEBUG - SMS receiver registered for API 26-32 with RECEIVER_NOT_EXPORTED flag");
                    }
                    else { // Older versions
                        activity.registerReceiver(smsReceiver, filter);
                        Log.d(TAG, "✅ DEBUG - SMS receiver registered for older APIs");
                    }
                }
            });
            
            task.addOnFailureListener(new OnFailureListener() {
                @Override
                public void onFailure(Exception e) {
                    Log.e(TAG, "❌ DEBUG - SMS Retriever task failed: " + e.getMessage());
                    // Consider sending a message to Unity about the failure
                     activity.runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                             com.unity3d.player.UnityPlayer.UnitySendMessage("OtpManager", "OnOtpError", "SMS_RETRIEVER_START_FAILED");
                        }
                    });
                }
            });
            
        } catch (Exception e) {
            Log.e(TAG, "❌ DEBUG - Exception in StartSmsListener: " + e.getMessage());
        }
    }

    public void StopSmsListener() {
        if (activity != null && smsReceiver != null) {
            try {
                activity.unregisterReceiver(smsReceiver);
                Log.d(TAG, "✅ DEBUG - SMS receiver unregistered successfully");
            } catch (Exception e) {
                Log.e(TAG, "❌ DEBUG - Exception in StopSmsListener: " + e.getMessage());
            } finally {
                smsReceiver = null;
            }
        }
    }
}
