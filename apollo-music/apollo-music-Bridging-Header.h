//
//  discord-amusic-richpresence-Bridging-Header.h
//  discord-amusic-richpresence
//
//  Created by Pedro Cavaleiro on 01/11/2020.
//
//#import <Foundation/Foundation.h>
//#import <AppKit/AppKit.h>

#import "MusicBridge.h"

//typedef void (^MRMediaRemoteGetNowPlayingInfoBlock)(NSDictionary *info);
//typedef void (^MRMediaRemoteGetNowPlayingClientBlock)(id clientObj);
//typedef void (^MRMediaRemoteGetNowPlayingApplicationIsPlayingBlock)(BOOL playing);
//
//extern void MRMediaRemoteRegisterForNowPlayingNotifications(dispatch_queue_t queue);
//extern void MRMediaRemoteGetNowPlayingClient(dispatch_queue_t queue, MRMediaRemoteGetNowPlayingClientBlock block);
//extern void MRMediaRemoteGetNowPlayingClients(dispatch_queue_t queue, MRMediaRemoteGetNowPlayingClientBlock block);
//extern void MRMediaRemoteGetNowPlayingInfo(dispatch_queue_t queue, MRMediaRemoteGetNowPlayingInfoBlock block);
//extern void MRMediaRemoteGetNowPlayingApplicationIsPlaying(dispatch_queue_t queue, MRMediaRemoteGetNowPlayingApplicationIsPlayingBlock block);
//
//extern NSString *MRNowPlayingClientGetBundleIdentifier(id clientObj);
//extern NSString *MRNowPlayingClientGetParentAppBundleIdentifier(id clientObj);
//
//extern NSString *kMRMediaRemoteNowPlayingApplicationIsPlayingDidChangeNotification;
//extern NSString *kMRMediaRemoteNowPlayingApplicationClientStateDidChange;
//extern NSString *kMRNowPlayingPlaybackQueueChangedNotification;
//extern NSString *kMRPlaybackQueueContentItemsChangedNotification;
//extern NSString *kMRMediaRemoteNowPlayingApplicationDidChangeNotification;
//
//extern NSString *kMRMediaRemoteNowPlayingInfoAlbum;
//extern NSString *kMRMediaRemoteNowPlayingInfoArtist;
//extern NSString *kMRMediaRemoteNowPlayingInfoTitle;
//extern NSString *kMRMediaRemoteNowPlayingInfoDuration;
//extern NSString *kMRMediaRemoteNowPlayingInfoTotalQueueCount;
//extern NSString *kMRMediaRemoteNowPlayingInfoTrackNumber;
//extern NSString *kMRMediaRemoteNowPlayingInfoTotalTrackCount;
//extern NSString *kMRMediaRemoteNowPlayingInfoGenre;
//extern NSString *kMRMediaRemoteNowPlayingInfoElapsedTime;
//extern NSString *kMRMediaRemoteNowPlayingInfoTimestamp;
//extern NSString *kMRMediaRemoteNowPlayingInfoPlaybackRate;
//
//typedef enum {
//    kMRPlay = 0,
//    kMRPause = 1,
//    kMRTogglePlayPause = 2,
//    kMRNextTrack = 4,
//    kMRPreviousTrack = 5,
//} MRCommand;
//
//extern Boolean MRMediaRemoteSendCommand(MRCommand command, id userInfo);
