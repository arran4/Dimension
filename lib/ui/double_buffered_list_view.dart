import 'package:flutter/material.dart';

/// Flutter replacement for the WinForms `DoubleBufferedListView`.
///
/// Flutter already paints with a retained, GPU-accelerated pipeline, so
/// explicit double buffering is not needed. This widget wraps a lazily built
/// list and keeps API usage simple for the ongoing UI port.
class DoubleBufferedListView extends StatelessWidget {
  const DoubleBufferedListView({
    super.key,
    required this.itemCount,
    required this.itemBuilder,
    this.padding,
    this.controller,
  });

  final int itemCount;
  final IndexedWidgetBuilder itemBuilder;
  final EdgeInsetsGeometry? padding;
  final ScrollController? controller;

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      controller: controller,
      padding: padding,
      itemCount: itemCount,
      itemBuilder: itemBuilder,
    );
  }
}
