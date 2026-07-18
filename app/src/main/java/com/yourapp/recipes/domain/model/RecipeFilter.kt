data class RecipeFilter(
    val searchQuery: String = "",
    val category: Category? = null,
    val difficulty: Difficulty? = null,
    val maxCookingTime: Int? = null,
    val onlyFavorites: Boolean = false,
    val sortBy: SortOption = SortOption.DATE_ADDED
)
