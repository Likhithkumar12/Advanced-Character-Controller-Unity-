#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface ToastManager : NSObject
+ (instancetype)shared;
- (void)showToastWithMessage:(NSString *)message duration:(double)duration;
@end

@implementation ToastManager

+ (instancetype)shared {
    static ToastManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (void)showToastWithMessage:(NSString *)message duration:(double)duration {
    dispatch_async(dispatch_get_main_queue(), ^{
        UIWindow *window = [self getKeyWindow];
        if (window) {
            [self createAndShowToastWithMessage:message duration:duration inWindow:window];
        }
    });
}

- (UIWindow *)getKeyWindow {
    UIWindow *keyWindow = nil;
    
    // iOS 13+ approach
    if (@available(iOS 13.0, *)) {
        for (UIWindowScene *windowScene in [UIApplication sharedApplication].connectedScenes) {
            if (windowScene.activationState == UISceneActivationStateForegroundActive) {
                for (UIWindow *window in windowScene.windows) {
                    if (window.isKeyWindow) {
                        keyWindow = window;
                        break;
                    }
                }
                if (keyWindow) break;
            }
        }
    }
    
    // Fallback for older iOS versions
    if (!keyWindow) {
        keyWindow = [UIApplication sharedApplication].keyWindow;
    }
    
    // Additional fallback
    if (!keyWindow) {
        keyWindow = [UIApplication sharedApplication].windows.firstObject;
    }
    
    return keyWindow;
}

- (void)createAndShowToastWithMessage:(NSString *)message duration:(double)duration inWindow:(UIWindow *)window {
    // Remove any existing toast
    for (UIView *subview in window.subviews) {
        if (subview.tag == 999999) {
            [subview removeFromSuperview];
        }
    }
    
    // Create toast container
    UIView *toastContainer = [[UIView alloc] init];
    toastContainer.tag = 999999;
    toastContainer.backgroundColor = [[UIColor blackColor] colorWithAlphaComponent:0.8];
    toastContainer.layer.cornerRadius = 10;
    toastContainer.clipsToBounds = YES;
    toastContainer.translatesAutoresizingMaskIntoConstraints = NO;
    
    // Create toast label
    UILabel *toastLabel = [[UILabel alloc] init];
    toastLabel.textColor = [UIColor whiteColor];
    toastLabel.textAlignment = NSTextAlignmentCenter;
    toastLabel.font = [UIFont systemFontOfSize:16];
    toastLabel.text = message;
    toastLabel.numberOfLines = 0;
    toastLabel.lineBreakMode = NSLineBreakByWordWrapping;
    toastLabel.translatesAutoresizingMaskIntoConstraints = NO;
    
    // Add label to container
    [toastContainer addSubview:toastLabel];
    
    // Label constraints
    [NSLayoutConstraint activateConstraints:@[
        [toastLabel.topAnchor constraintEqualToAnchor:toastContainer.topAnchor constant:12],
        [toastLabel.bottomAnchor constraintEqualToAnchor:toastContainer.bottomAnchor constant:-12],
        [toastLabel.leadingAnchor constraintEqualToAnchor:toastContainer.leadingAnchor constant:16],
        [toastLabel.trailingAnchor constraintEqualToAnchor:toastContainer.trailingAnchor constant:-16]
    ]];
    
    // Add container to window
    [window addSubview:toastContainer];
    
    // Container constraints
    NSLayoutConstraint *bottomConstraint;
    if (@available(iOS 11.0, *)) {
        bottomConstraint = [toastContainer.bottomAnchor constraintEqualToAnchor:window.safeAreaLayoutGuide.bottomAnchor constant:-50];
    } else {
        bottomConstraint = [toastContainer.bottomAnchor constraintEqualToAnchor:window.bottomAnchor constant:-70];
    }
    
    [NSLayoutConstraint activateConstraints:@[
        [toastContainer.centerXAnchor constraintEqualToAnchor:window.centerXAnchor],
        bottomConstraint,
        [toastContainer.leadingAnchor constraintGreaterThanOrEqualToAnchor:window.leadingAnchor constant:20],
        [toastContainer.trailingAnchor constraintLessThanOrEqualToAnchor:window.trailingAnchor constant:-20],
        [toastContainer.widthAnchor constraintLessThanOrEqualToConstant:300]
    ]];
    
    // Initial state (hidden)
    toastContainer.alpha = 0;
    toastContainer.transform = CGAffineTransformMakeScale(0.8, 0.8);
    
    // Animate in
    [UIView animateWithDuration:0.3 animations:^{
        toastContainer.alpha = 1;
        toastContainer.transform = CGAffineTransformIdentity;
    } completion:^(BOOL finished) {
        // Animate out after duration
        [UIView animateWithDuration:0.3 delay:duration options:0 animations:^{
            toastContainer.alpha = 0;
            toastContainer.transform = CGAffineTransformMakeScale(0.8, 0.8);
        } completion:^(BOOL finished) {
            [toastContainer removeFromSuperview];
        }];
    }];
}

@end

extern "C" {
    // Function to show toast with custom duration
    void _showToast(const char* message, double duration) {
        if (message == NULL) return;
        
        NSString *messageString = [NSString stringWithUTF8String:message];
        [[ToastManager shared] showToastWithMessage:messageString duration:duration];
    }
}
