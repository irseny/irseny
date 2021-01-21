// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

function EquipmentRenameController($element) {
	var self = this;

	this.$postLink = function() {
		$element[0].focus();
		$element[0].select();

		$element[0].addEventListener("focusout", self.listenFocusLost);
		$element[0].addEventListener("keydown", self.listenFinishKey);
	};
	this.onRenameFinish = function(applyNewName) {
		var nextName = $element[0].value;
		var event = new CustomEvent("rename-finish", {
			detail: {
				cancel: !applyNewName || !nextName,
				name: nextName ? nextName : undefined
			}
		});
		$element[0].dispatchEvent(event);
	};
	this.listenFocusLost = function(ev) {
		self.onRenameFinish(false);
	};
	this.listenFinishKey = function(ev) {
		if (ev.keyCode == 13) {
			self.onRenameFinish(true);
		}
		if (ev.keyCode == 27) {
			self.onRenameFinish(false);
		}
	};
}
EquipmentRenameController.$inject = ["$element"];

var module = angular.module("equipmentRename");
module.directive("equipmentRename", function($parse) {
	return {
		restrict: 'A',
		controller: EquipmentRenameController,
		scope: false,
		link: function(scope, iElement, iAttrs, ctrl) {
			// use the optionally given name expression as preliminary value
			// of the input field
			if (iAttrs["equipmentRename"] != undefined) {
				var handler = $parse(iAttrs["equipmentRename"]);
				if (handler == undefined) {
					iElement[0].value = nameExpr;
				} else {
					var currentName = handler(scope);
					if (currentName != undefined) {
						iElement[0].value = currentName;
					}
				}
			}
		}
	};
});