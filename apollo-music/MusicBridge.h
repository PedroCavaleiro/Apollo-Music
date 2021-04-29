//
//  MusicHelper.h
//  apollo-music
//
//  Created by Pedro Cavaleiro on 05/03/2021.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface MusicBridge : NSObject
- (NSDictionary<NSString *, NSObject *> *)currentTrackInfo;
- (BOOL)isPaused;
- (double)playerPosition;
@end

NS_ASSUME_NONNULL_END
