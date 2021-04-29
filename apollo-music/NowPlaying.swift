//
//  NowPlaying.swift
//  discord-amusic-richpresence
//
//  Created by Pedro Cavaleiro on 01/11/2020.
//

import Foundation

//class NowPlaying {
//    var appBundleIdentifier: String? = nil
//    var duration, elapsedTime: Double?
//    var album, genre, title, artist: String?
//    var albumTrackCount, songPositionInAlbum, inQueueCount: Int?
//    var startTimeStamp: Date?
//    var playing: Bool = false
//}

struct NowPlaying {
    let id: Int, title: String, artist: String?, album: String?, isPaused: Bool, startDate: Date
    
    private static let bridge = MusicBridge()
    static var current: NowPlaying? {
        let trackInfo = bridge.currentTrackInfo()
        guard let id = trackInfo["id"] as? Int, let title = trackInfo["name"] as? String else { return nil }
        
        let position = bridge.playerPosition()
        
        return NowPlaying(
            id: id, title: title,
            artist: trackInfo["artist"] as? String,
            album: trackInfo["album"] as? String,
            isPaused: bridge.isPaused(),
            startDate: Date() - position)
    }
}
