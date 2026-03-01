import 'package:dimension/ui/desktop_context_menu.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('buildActions creates refresh/detail menu entries', () {
    final controller = DesktopContextMenuController();
    var refreshCalls = 0;
    var detailCalls = 0;

    final actions = controller.buildActions(
      section: 'Peers',
      onRefresh: () => refreshCalls++,
      onOpenDetails: () => detailCalls++,
    );

    expect(actions.map((a) => a.label), <String>[
      'Refresh Peers',
      'Open Peers details',
    ]);

    actions[0].onSelected();
    actions[1].onSelected();

    expect(refreshCalls, 1);
    expect(detailCalls, 1);
  });
}
