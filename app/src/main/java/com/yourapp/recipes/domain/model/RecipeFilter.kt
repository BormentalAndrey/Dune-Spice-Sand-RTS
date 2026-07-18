data class RecipeFilter(
    val searchQuery: String = "",
    val category: Category? = null,
    val difficulty: Difficulty? = null,
    val maxCookingTime: Int? = null,
    val onlyFavorites: Boolean = false,
    val sortBy: SortOption = SortOption.DATE_ADDED
)

enum class SortOption(val displayName: String) {
    DATE_ADDED("По дате добавления"),
    TITLE("По названию"),
    COOKING_TIME("По времени приготовления"),
    CALORIES("По калорийности")
}
