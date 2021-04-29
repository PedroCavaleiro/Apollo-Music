//
//  AppDelegate.swift
//  discord-amusic-richpresence
//
//  Created by Pedro Cavaleiro on 01/11/2020.
//

import Cocoa
import SwordRPC
import LaunchAtLogin

@NSApplicationMain
class AppDelegate: NSObject, NSApplicationDelegate, NSMenuDelegate {
    
    var statusItem: NSStatusItem?
    @IBOutlet weak var menu: NSMenu!
    @IBOutlet weak var connectionStatus: NSMenuItem!
    @IBOutlet weak var showEmojisToggle: NSMenuItem!
    @IBOutlet weak var launchAtLoginToggle: NSMenuItem!
    
    var rpc: SwordRPC?
    
    var rcpConnected: Bool = false
    var rpcError: Bool = false
    var isStartup: Bool = true
    
    @IBAction func openAboutWindow(_ sender: Any) {
        let myWindowController = NSStoryboard(name: "Main", bundle: nil).instantiateController(withIdentifier: "aboutWindow") as! NSWindowController
        myWindowController.showWindow(self)
        NSApp.activate(ignoringOtherApps: true)
    }
    
    func applicationDidFinishLaunching(_ aNotification: Notification) {
        showEmojisToggle.state = Settings.shared.showEmojis ? NSControl.StateValue.on : NSControl.StateValue.off
        launchAtLoginToggle.state = LaunchAtLogin.isEnabled ? NSControl.StateValue.on : NSControl.StateValue.off
        print(NowPlaying.current)
//        NotificationCenter.default.addObserver(self, selector: #selector(mediaPlaybackChanged(_:)), name: MediaPlayerHelper.kNowPlayingItemDidChange, object: nil)
//        NotificationCenter.default.addObserver(self, selector: #selector(playingStatusChanged(_:)), name: MediaPlayerHelper.kNowPlayingStatusChange, object: nil)
//        configureRPC()
    }
    
//    func configureRPC() {
//        rpc = SwordRPC(appId: "772579806515691560", handlerInterval: 500)
//        rpc!.onConnect { rpc in
//            self.isStartup = false
//            self.rcpConnected = true
//            self.connectionStatus.title = "Connected to Discord"
//            if MediaPlayerHelper.shared.nowPlayingItem.playing {
//                self.configureRichPresence()
//            }
//        }
//        rpc!.onDisconnect { rpc, code, msg in
//            self.rcpConnected = false
//            self.connectionStatus.title = "Disconnected from Discord"
//            print("Disconnected \(String(describing: msg))")
//        }
//
//        rpc!.onError { rpc, code, msg in
//            self.connectionStatus.title = "Error connecting to Discord (\(code))"
//            self.rpcError = true
//            print("Error \(msg)")
//        }
//
//        rpc!.connect()
//    }

    func applicationWillTerminate(_ aNotification: Notification) {
        // Insert code here to tear down your application
    }

    override func awakeFromNib() {
        super.awakeFromNib()
     
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem?.button?.image = NSImage(named: "icons8-itunes-16")
        statusItem?.button?.imageScaling = NSImageScaling.scaleProportionallyDown
        if let menu = menu {
            statusItem?.menu = menu
            menu.delegate = self
        }
    }
    
//    @objc func playingStatusChanged(_ notification: Notification) {
//        if MediaPlayerHelper.shared.nowPlayingItem.playing {
//            configureRichPresence()
//        } else {
//            rpc!.disconnect()
//        }
//    }
//
//    @objc func mediaPlaybackChanged(_ notification: Notification) {
//        if rcpConnected {
//            configureRichPresence()
//        }
//    }
//
//    func configureRichPresence() {
//        if rcpConnected {
//            var presence = RichPresence()
//            presence.details = "\(Settings.shared.showEmojis ? "🎵 " : "")\(MediaPlayerHelper.shared.nowPlayingItem.title ?? "  ")"
//            presence.state = "\(Settings.shared.showEmojis ? "👤 " : "by ")\(MediaPlayerHelper.shared.nowPlayingItem.artist ?? "  ")"
//            presence.assets.largeImage = "apple_music_icon_rounded"
//            presence.assets.largeText = "Apple Music"
//            self.rpc!.setPresence(presence)
//        } else {
//            if !isStartup {
//                rpc = nil
//                DispatchQueue.main.async {
//                    self.configureRPC()
//                }
//            }
//        }
//    }

    @IBAction func showEmojis(_ sender: Any) {
        if (sender as! NSMenuItem).state == .on {
            Settings.shared.showEmojis = false
            
        } else {
            Settings.shared.showEmojis = true
        }
        (sender as! NSMenuItem).state = Settings.shared.showEmojis ? NSControl.StateValue.on : NSControl.StateValue.off
//        configureRichPresence()
    }
    
    @IBAction func launchAtLogin(_ sender: Any) {
        if (sender as! NSMenuItem).state == .on {
            LaunchAtLogin.isEnabled = false
            
        } else {
            LaunchAtLogin.isEnabled = true
        }
        (sender as! NSMenuItem).state = LaunchAtLogin.isEnabled ? NSControl.StateValue.on : NSControl.StateValue.off
    }
    
}

