function MainFrameController() {
	this.sayHello = "Hello, World!";
	// tabs: cameras, trackers, devices, config
	this.availableTabs = [ "Cameras", "Trackers", "Devices", "Config" ];
	this.activeTab = "Cameras";
}

var module = angular.module("mainPage");
module.component("mainPage", {
	templateUrl: "/main-page/main-page.template.html",
	controller: MainFrameController
});
