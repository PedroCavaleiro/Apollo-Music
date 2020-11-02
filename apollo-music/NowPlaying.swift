//
//  NowPlaying.swift
//  discord-amusic-richpresence
//
//  Created by Pedro Cavaleiro on 01/11/2020.
//

import Foundation

class NowPlaying {
    var appBundleIdentifier: String? = nil
    var duration, elapsedTime: Double?
    var album, genre, title, artist: String?
    var albumTrackCount, songPositionInAlbum, inQueueCount: Int?
    var startTimeStamp: Date?
    var playing: Bool = false
}
