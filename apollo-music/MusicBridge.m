//
//  MusicHelper.m
//  apollo-music
//
//  Created by Pedro Cavaleiro on 05/03/2021.
//

#import "MusicBridge.h"
#import <ScriptingBridge/ScriptingBridge.h>
#import "MusicAPI.h"

@implementation MusicBridge {
    MusicApplication *app;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        app = [SBApplication applicationWithBundleIdentifier:@"com.apple.Music"] ?: [SBApplication applicationWithBundleIdentifier:@"com.apple.iTunes"];
    }
    return self;
}

- (NSDictionary<NSString *, NSObject *> *)currentTrackInfo {
    MusicTrack *track = [app currentTrack];
//    MusicSubscriptionPlaylist *track = app.playlists
//    NSLog(@"%hhd", app.EQEnabled); for bool
//    NSLog(@"%@", app.);
//    for (MusicPlaylist* value in app.playlists) {
//        NSLog(@"%@", value.);
//    }
    return track.properties;
}

- (BOOL)isPaused {
    return app.playerState == MusicEPlSPaused;
}

- (double)playerPosition {
    return app.playerPosition;
}

@end
