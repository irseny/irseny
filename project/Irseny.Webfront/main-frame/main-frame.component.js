function MainFrameController() {
	this.sayHello = "Hello, World!";
}

var module = angular.module("mainFrame");
module.component("mainFrame", {
	templateUrl: "/main-frame/main-frame.template.html",
	controller: MainFrameController
});
