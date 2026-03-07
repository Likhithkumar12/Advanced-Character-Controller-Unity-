package otpreader;

import android.content.Context;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.util.Base64;
import android.util.Log;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.ArrayList;
import java.util.Arrays;

import com.unity3d.player.UnityPlayer;

public class AppHashHelper {
    private static final String TAG = "AppHashHelper";
    private static final String HASH_TYPE = "SHA-256";
    private static final int NUM_HASHED_BYTES = 9;
    private static final int NUM_BASE64_CHAR = 11;

    public static void getAppHash() {
        Context context = UnityPlayer.currentActivity;
        if (context == null) {
            UnityPlayer.UnitySendMessage("OtpManager", "TestUnityMessage", "ERROR: Context is null");
            return;
        }

        String packageName = context.getPackageName();

        try {
            PackageManager packageManager = context.getPackageManager();
            Signature[] signatures = packageManager.getPackageInfo(packageName,
                    PackageManager.GET_SIGNATURES).signatures;

            for (Signature signature : signatures) {
                String hash = hash(packageName, signature.toCharsString());
                Log.d(TAG, "✅ DEBUG - Generated App Hash: " + hash);
                
                // Send the hash to Unity with debug info
                UnityPlayer.UnitySendMessage("OtpManager", "TestUnityMessage", hash);
            }
        } catch (Exception e) {
            Log.e(TAG, "❌ DEBUG - Error getting app hash: " + e.getMessage());
            UnityPlayer.UnitySendMessage("OtpManager", "TestUnityMessage", "ERROR: " + e.getMessage());
        }
    }

    private static String hash(String packageName, String signature) {
        String appInfo = packageName + " " + signature;
        try {
            MessageDigest messageDigest = MessageDigest.getInstance(HASH_TYPE);
            messageDigest.update(appInfo.getBytes(StandardCharsets.UTF_8));
            byte[] hashSignature = messageDigest.digest();

            // truncated into NUM_HASHED_BYTES
            byte[] truncatedHash = Arrays.copyOfRange(hashSignature, 0, NUM_HASHED_BYTES);
            // encode into Base64
            String base64Hash = Base64.encodeToString(truncatedHash, Base64.NO_PADDING | Base64.NO_WRAP);
            base64Hash = base64Hash.substring(0, NUM_BASE64_CHAR);

            return base64Hash;
        } catch (NoSuchAlgorithmException e) {
            // Silently handle error
        }
        return null;
    }
}
