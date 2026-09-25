/// Shape of every list response: {items, page, pageSize, total} (`PagedResult<T>` in C#).
class PagedResult<T> {
  const PagedResult({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.total,
  });

  factory PagedResult.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) parseItem,
  ) => PagedResult(
    items: (json['items'] as List<dynamic>)
        .map((e) => parseItem(e as Map<String, dynamic>))
        .toList(),
    page: json['page'] as int,
    pageSize: json['pageSize'] as int,
    total: json['total'] as int,
  );

  final List<T> items;
  final int page;
  final int pageSize;
  final int total;
}
