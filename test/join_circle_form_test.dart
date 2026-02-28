import 'dart:io';

import 'package:dimension/model/bootstrap.dart';
import 'package:dimension/ui/join_circle_form.dart';
import 'package:flutter_test/flutter_test.dart';

class _Service implements JoinCircleService {
  String? bootstrapInput;
  String? kadInput;

  @override
  bool isKademliaReady = false;

  @override
  Future<List<InternetAddressEndpoint>> joinBootstrap(String address) async {
    bootstrapInput = address;
    return [
      InternetAddressEndpoint(InternetAddress.loopbackIPv4, 4040),
    ];
  }

  @override
  Future<List<InternetAddressEndpoint>> lookupKademlia(String query) async {
    kadInput = query;
    return const [];
  }
}

void main() {
  test('joinCircle uses bootstrap service and forwards normalized LAN input', () async {
    final service = _Service();
    List<InternetAddressEndpoint>? addedEndpoints;
    String? addedInput;
    CircleType? addedType;

    final controller = JoinCircleController(
      service: service,
      addInternetCircle: (endpoints, input, type) {
        addedEndpoints = endpoints;
        addedInput = input;
        addedType = type;
      },
    );

    await controller.joinCircle('http://lan', CircleType.bootstrap);

    expect(service.bootstrapInput, isNull);
    expect(addedInput, 'LAN');
    expect(addedType, CircleType.bootstrap);
    expect(addedEndpoints, isEmpty);
  });

  test('joinCircle calls kademlia lookup when requested', () async {
    final service = _Service()..isKademliaReady = true;

    final controller = JoinCircleController(
      service: service,
      addInternetCircle: (_, __, ___) {},
    );

    await controller.joinCircle(' #Room42 ', CircleType.kademlia);

    expect(service.kadInput, '#room42');
  });
}
