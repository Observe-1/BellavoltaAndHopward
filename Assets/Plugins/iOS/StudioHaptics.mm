// Original native bridge for restrained UIKit haptics. No third-party code.
#import <UIKit/UIKit.h>

extern "C" {
    void StudioHapticImpact(int style)
    {
        UIImpactFeedbackStyle s = style == 0 ? UIImpactFeedbackStyleLight : UIImpactFeedbackStyleMedium;
        UIImpactFeedbackGenerator *g = [[UIImpactFeedbackGenerator alloc] initWithStyle:s];
        [g prepare];
        [g impactOccurred];
    }

    void StudioHapticSuccess()
    {
        UINotificationFeedbackGenerator *g = [[UINotificationFeedbackGenerator alloc] init];
        [g prepare];
        [g notificationOccurred:UINotificationFeedbackTypeSuccess];
    }
}
