class DateFormatter {
  static String formatDate(DateTime date) {
    if (date.year == DateTime(1).year) {
      return 'Updating...';
    }

    final age = DateTime.now().difference(date);

    if (age.inSeconds < 0) {
      return '';
    }
    if (age.inMinutes < 1) {
      return 'Just Now';
    }
    if (age.inMinutes == 1) {
      return '1 Minute Ago';
    }
    if (age.inMinutes < 60) {
      return '${age.inMinutes} Minutes Ago';
    }
    if (age.inHours == 1) {
      return '1 Hour Ago';
    }
    if (age.inHours < 24) {
      return '${age.inHours} Hours Ago';
    }
    if (age.inDays == 1) {
      return '1 Day Ago';
    }
    if (age.inDays < 7) {
      return '${age.inDays} Days Ago';
    }

    final weeks = age.inDays ~/ 7;
    if (weeks == 1) {
      return '1 Week Ago';
    }
    if (weeks < 4) {
      return '$weeks Weeks Ago';
    }
    if (weeks == 4) {
      return '1 Month Ago';
    }

    final months = age.inDays ~/ 28;
    if (age.inDays < 365) {
      return '$months Months Ago';
    }

    final years = age.inDays ~/ 365;
    if (years == 1) {
      return '1 Year Ago';
    }
    return '$years Years Ago';
  }

  const DateFormatter._();
}
