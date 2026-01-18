// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


            // JavaScript for filtering menu items

            document.querySelectorAll('#category-buttons button').forEach(btn => {
            btn.addEventListener('click', function () {
                document.querySelectorAll('#category-buttons button').forEach(b => b.classList.remove('active'));
                this.classList.add('active');

                const filter = this.getAttribute('data-filter');
                document.querySelectorAll('.menu-item').forEach(card => {
                    if (filter === 'all' || card.getAttribute('data-category') === filter) {
                        card.style.display = 'block';
                    } else {
                        card.style.display = 'none';
                    }
                });
            });
            });


            // JavaScript for searching menu items
            const searchBox = document.getElementById('searchBox');
            const noResults = document.getElementById("noResults");
                searchBox.addEventListener('keyup', function () {
                    const keyword = this.value.toLowerCase();
                    let visibleCount = 0;
                document.querySelectorAll('.menu-item').forEach(card => {
                    const name = card.getAttribute('data-name').toLowerCase();
                const category = card.getAttribute('data-category').toLowerCase();

                if (name.includes(keyword) || category.includes(keyword)) {
                    card.style.display = 'block';
                    visibleCount++;
                    } else {
                    card.style.display = 'none';
                    }
                });

                    //if no items are visible, show "No items found" message
                    noResults.style.display = visibleCount === 0 ? 'block' : 'none';
            });

