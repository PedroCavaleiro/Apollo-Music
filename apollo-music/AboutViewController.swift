//
//  AboutViewController.swift
//  Apollo Music
//
//  Created by Pedro Cavaleiro on 02/11/2020.
//

import Cocoa

class AboutViewController: NSViewController {

    @IBOutlet weak var versionLabel: NSTextField!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        let appVersion = Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String
        let appBuild = Bundle.main.infoDictionary!["CFBundleVersion"] as! String
        versionLabel.stringValue = "Version \(appVersion!) (\(appBuild))"
    }
    
}
