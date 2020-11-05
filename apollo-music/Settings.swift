//
//  Settings.swift
//  Apollo Music
//
//  Created by Pedro Cavaleiro on 05/11/2020.
//

import Foundation

class Settings {
    
    static let shared = Settings()
    
    private let settings = UserDefaults.standard
    
    private init() { }
    
    var showEmojis: Bool {
        get {
            return settings.bool(forKey: "showEmojis")
        }
        set {
            settings.setValue(newValue, forKey: "showEmojis")
        }
    }
    
}
