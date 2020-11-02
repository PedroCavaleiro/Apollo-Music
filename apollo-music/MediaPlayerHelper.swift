//
//  MediaPlayerHelper.swift
//  discord-amusic-richpresence
//
//  Created by Pedro Cavaleiro on 01/11/2020.
//

import Foundation

class MediaPlayerHelper {
    
    public static let kNowPlayingItemDidChange = Notification.Name(rawValue: "kNowPlayingItemDidChange")
    public static let kNowPlayingStatusChange = Notification.Name(rawValue: "kNowPlayingStatusChange")
    
    public let nowPlayingItem = NowPlaying()
    
    init() {
        MRMediaRemoteRegisterForNowPlayingNotifications(DispatchQueue.global(qos: .utility))
        updateCurrentPlayingApp()
        updateCurrentPlayingState()
        updateMediaContent()
        NotificationCenter.default.addObserver(self,
                                               selector: #selector(updateCurrentPlayingApp),
                                               name: NSNotification.Name.mrMediaRemoteNowPlayingApplicationDidChange,
                                               object: nil
        )
        NotificationCenter.default.addObserver(self,
                                               selector: #selector(updateMediaContent),
                                               name: NSNotification.Name.mrNowPlayingPlaybackQueueChanged,
                                               object: nil
        )
        NotificationCenter.default.addObserver(self,
                                               selector: #selector(updateMediaContent),
                                               name: NSNotification.Name.mrNowPlayingPlaybackQueueChanged,
                                               object: nil
        )
        NotificationCenter.default.addObserver(self,
                                               selector: #selector(updateMediaContent),
                                               name: NSNotification.Name.mrPlaybackQueueContentItemsChanged,
                                               object: nil
        )
        NotificationCenter.default.addObserver(self,
                                               selector: #selector(updateCurrentPlayingState),
                                               name: NSNotification.Name.mrMediaRemoteNowPlayingApplicationIsPlayingDidChange,
                                               object: nil
                )
    }
    
    deinit {
        NotificationCenter.default.removeObserver(NSNotification.Name.mrNowPlayingPlaybackQueueChanged)
        NotificationCenter.default.removeObserver(NSNotification.Name.mrNowPlayingPlaybackQueueChanged)
        NotificationCenter.default.removeObserver(NSNotification.Name.mrPlaybackQueueContentItemsChanged)
        NotificationCenter.default.removeObserver(NSNotification.Name.mrMediaRemoteNowPlayingApplicationIsPlayingDidChange)
    }
    
    @objc private func updateCurrentPlayingState() {
        MRMediaRemoteGetNowPlayingApplicationIsPlaying(DispatchQueue.global(qos: .utility), { isPlaying in
            if self.nowPlayingItem.appBundleIdentifier == nil {
                self.nowPlayingItem.playing = false
                NotificationCenter.default.post(name: MediaPlayerHelper.kNowPlayingStatusChange, object: false)
            }else {
                // Keep support only for macOS Music App
                if self.nowPlayingItem.appBundleIdentifier == "com.apple.Music" {
                    self.nowPlayingItem.playing = isPlaying
                    NotificationCenter.default.post(name: MediaPlayerHelper.kNowPlayingStatusChange, object: isPlaying)
                } else {
                    self.nowPlayingItem.playing = false
                    NotificationCenter.default.post(name: MediaPlayerHelper.kNowPlayingStatusChange, object: false)
                }
            }
        })
    }
    
    @objc private func updateCurrentPlayingApp() {
        MRMediaRemoteGetNowPlayingClients(DispatchQueue.global(qos: .utility), { clients in
            if let info = (clients as? [Any])?.last {
                if let appBundleIdentifier = MRNowPlayingClientGetBundleIdentifier(info) {
                    self.nowPlayingItem.appBundleIdentifier = appBundleIdentifier
                }else if let appBundleIdentifier = MRNowPlayingClientGetParentAppBundleIdentifier(info) {
                    self.nowPlayingItem.appBundleIdentifier = appBundleIdentifier
                }else {
                    self.nowPlayingItem.appBundleIdentifier = nil
                }
            }else {
                self.nowPlayingItem.appBundleIdentifier = nil
            }
        })
    }
    
    @objc private func updateMediaContent() {
        MRMediaRemoteGetNowPlayingInfo(DispatchQueue.global(qos: .utility), { info in
            if let information = info {
                self.nowPlayingItem.duration = information[kMRMediaRemoteNowPlayingInfoDuration] as? Double
                self.nowPlayingItem.elapsedTime = information[kMRMediaRemoteNowPlayingInfoElapsedTime] as? Double
                self.nowPlayingItem.album = information[kMRMediaRemoteNowPlayingInfoAlbum] as? String
                self.nowPlayingItem.genre = information[kMRMediaRemoteNowPlayingInfoGenre] as? String
                self.nowPlayingItem.title = information[kMRMediaRemoteNowPlayingInfoTitle] as? String
                self.nowPlayingItem.artist = information[kMRMediaRemoteNowPlayingInfoArtist] as? String
                self.nowPlayingItem.albumTrackCount = information[kMRMediaRemoteNowPlayingInfoTotalTrackCount] as? Int
                self.nowPlayingItem.songPositionInAlbum = information[kMRMediaRemoteNowPlayingInfoTrackNumber] as? Int
                self.nowPlayingItem.inQueueCount = information[kMRMediaRemoteNowPlayingInfoTotalQueueCount] as? Int
                self.nowPlayingItem.startTimeStamp = information[kMRMediaRemoteNowPlayingInfoTimestamp] as? Date
            }
            NotificationCenter.default.post(name: MediaPlayerHelper.kNowPlayingItemDidChange, object: nil)
        })
    }
    
}
